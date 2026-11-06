// SPDX-License-Identifier: Apache-2.0
//! Operator-only bootstrap file; its path, never secret contents, reaches open.
use super::Error;
use std::fs::{self, File, OpenOptions};
use std::io::Write;
use std::os::unix::fs::{MetadataExt, OpenOptionsExt, PermissionsExt};
use std::path::PathBuf;

pub(super) struct Bootstrap {
    directory: PathBuf,
    directory_stamp: (u64, u64),
    stamp: (u64, u64),
}
impl Bootstrap {
    pub(super) fn create(secret: &str) -> Result<Self, Error> {
        if secret.len() != 64
            || !secret
                .bytes()
                .all(|byte| byte.is_ascii_digit() || (b'a'..=b'f').contains(&byte))
        {
            return Err(Error::BootstrapUnavailable);
        }
        let directory = tempfile::Builder::new()
            .prefix("openknx-operator-")
            .permissions(fs::Permissions::from_mode(0o700))
            .tempdir_in("/private/tmp")
            .map_err(|_| Error::BootstrapUnavailable)?;
        let metadata =
            fs::symlink_metadata(directory.path()).map_err(|_| Error::BootstrapUnavailable)?;
        if metadata.uid() != nix::unistd::geteuid().as_raw() || metadata.mode() & 0o7777 != 0o700 {
            return Err(Error::BootstrapUnavailable);
        }
        // An inherited allow ACL could expose this secret despite POSIX mode.
        let parent = File::open(directory.path()).map_err(|_| Error::BootstrapUnavailable)?;
        crate::macos_config::no_granting_acl(&parent).map_err(|_| Error::BootstrapUnavailable)?;
        let directory_stamp = (metadata.dev(), metadata.ino());
        let path = directory.path().join("bootstrap.html");
        let mut file = OpenOptions::new()
            .write(true)
            .create_new(true)
            .mode(0o600)
            .open(&path)
            .map_err(|_| Error::BootstrapUnavailable)?;
        crate::macos_config::no_granting_acl(&file).map_err(|_| Error::BootstrapUnavailable)?;
        let metadata = file.metadata().map_err(|_| Error::BootstrapUnavailable)?;
        if !metadata.is_file()
            || metadata.uid() != nix::unistd::geteuid().as_raw()
            || metadata.mode() & 0o7777 != 0o600
            || metadata.nlink() != 1
        {
            return Err(Error::BootstrapUnavailable);
        }
        let html = format!(
            "<!doctype html><meta charset=utf-8><meta name=referrer content=no-referrer><title>OpenKNX Studio operator</title><script>location.replace('http://localhost:15300/#{secret}')</script>"
        );
        file.write_all(html.as_bytes())
            .and_then(|_| file.flush())
            .map_err(|_| Error::BootstrapUnavailable)?;
        Ok(Self {
            directory: directory.keep(),
            directory_stamp,
            stamp: (metadata.dev(), metadata.ino()),
        })
    }
    pub(super) fn open_safari(&self) -> Result<(), Error> {
        let mut child = std::process::Command::new("/usr/bin/open")
            .args(["-b", "com.apple.Safari"])
            .arg(self.directory.join("bootstrap.html"))
            .stdin(std::process::Stdio::null())
            .stdout(std::process::Stdio::null())
            .stderr(std::process::Stdio::null())
            .spawn()
            .map_err(|_| Error::BootstrapUnavailable)?;
        let deadline = std::time::Instant::now() + std::time::Duration::from_secs(2);
        loop {
            if let Some(status) = child.try_wait().map_err(|_| Error::BootstrapUnavailable)? {
                return if status.success() {
                    Ok(())
                } else {
                    Err(Error::BootstrapUnavailable)
                };
            }
            if std::time::Instant::now() >= deadline {
                let _ = child.kill();
                let _ = child.wait();
                return Err(Error::BootstrapUnavailable);
            }
            std::thread::sleep(std::time::Duration::from_millis(10));
        }
    }

    pub(super) fn remove_file(&self) -> Result<(), Error> {
        let path = self.directory.join("bootstrap.html");
        let metadata = fs::symlink_metadata(&path).map_err(|_| Error::BootstrapUnavailable)?;
        if !metadata.is_file() || (metadata.dev(), metadata.ino()) != self.stamp {
            return Err(Error::BootstrapUnavailable);
        }
        fs::remove_file(path).map_err(|_| Error::BootstrapUnavailable)
    }
}

impl Drop for Bootstrap {
    fn drop(&mut self) {
        // Never recursively delete or follow a substituted path. Unexpected
        // contents or identity retain quarantine for the operator instead.
        let Ok(metadata) = fs::symlink_metadata(&self.directory) else {
            return;
        };
        if !metadata.is_dir() || (metadata.dev(), metadata.ino()) != self.directory_stamp {
            return;
        }
        let path = self.directory.join("bootstrap.html");
        match fs::symlink_metadata(&path) {
            Ok(metadata)
                if metadata.is_file() && (metadata.dev(), metadata.ino()) == self.stamp =>
            {
                if fs::remove_file(path).is_err() {
                    return;
                }
            }
            Err(error) if error.kind() == std::io::ErrorKind::NotFound => (),
            _ => return,
        }
        let _ = fs::remove_dir(&self.directory);
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    #[test]
    fn bootstrap_is_private_exclusive_and_cleanup_preserves_substituted_file() {
        let item = Bootstrap::create(&"a".repeat(64)).unwrap();
        let path = item.directory.join("bootstrap.html");
        assert_eq!(
            fs::metadata(&item.directory).unwrap().mode() & 0o7777,
            0o700
        );
        assert_eq!(fs::metadata(&path).unwrap().mode() & 0o7777, 0o600);
        let text = fs::read_to_string(&path).unwrap();
        assert!(text.contains(&format!("http://localhost:15300/#{}", "a".repeat(64))));
        assert!(text.contains("no-referrer"));
        let original = item.directory.join("original");
        fs::rename(&path, &original).unwrap();
        fs::write(&path, b"replacement").unwrap();
        assert_eq!(item.remove_file(), Err(Error::BootstrapUnavailable));
        let directory = item.directory.clone();
        drop(item);
        assert_eq!(fs::read(&path).unwrap(), b"replacement");
        fs::remove_file(path).unwrap();
        fs::remove_file(original).unwrap();
        fs::remove_dir(directory).unwrap();
        assert!(Bootstrap::create("not-a-secret").is_err());
    }
    #[test]
    fn normal_bootstrap_cleanup_removes_only_its_empty_private_directory() {
        let item = Bootstrap::create(&"b".repeat(64)).unwrap();
        let directory = item.directory.clone();
        item.remove_file().unwrap();
        drop(item);
        assert!(!directory.exists());
    }
}
