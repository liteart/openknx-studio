// SPDX-License-Identifier: Apache-2.0
//! Kernel-observed macOS Unix-socket credentials, not a protected host proof.
//!
//! The service still has no installed privilege boundary, trusted policy store,
//! operator display, enrollment or executor. Caller-supplied IDs must never be
//! loaded from IPC and are not an alternative to protected configuration.

use nix::unistd::getpeereid;
use std::os::unix::net::UnixStream;

#[derive(Debug, PartialEq, Eq)]
pub enum PeerError {
    CredentialsUnavailable,
    SharedPrincipal,
    UnexpectedPrincipal,
}

/// OS-observed connect-time effective UID/GID. This is connection identity only;
/// it cannot establish operator consent or grant a device capability.
///
/// ```compile_fail
/// use task153_broker_core::macos_peer::PeerIdentity;
/// let forged = PeerIdentity { uid: 1, gid: 1 };
/// ```
#[derive(Debug, PartialEq, Eq)]
pub struct PeerIdentity {
    uid: u32,
    gid: u32,
}

impl PeerIdentity {
    pub fn uid(&self) -> u32 {
        self.uid
    }

    pub fn gid(&self) -> u32 {
        self.gid
    }
}

/// The expected identity must ultimately come from the protected service's
/// configuration. Constructing this policy does not validate that source.
pub struct ExpectedOperator {
    uid: u32,
    gid: u32,
    access: AccountAccess,
}

enum AccountAccess {
    Separate { agent_uid: u32 },
    SameAccount,
}

impl ExpectedOperator {
    pub(crate) fn uid(&self) -> u32 {
        self.uid
    }

    pub(crate) fn gid(&self) -> u32 {
        self.gid
    }

    pub fn new(uid: u32, gid: u32, agent_uid: u32) -> Result<Self, PeerError> {
        if uid == agent_uid {
            return Err(PeerError::SharedPrincipal);
        }
        Ok(Self {
            uid,
            gid,
            access: AccountAccess::Separate { agent_uid },
        })
    }

    /// Permit the configured account to submit assertions for a previously
    /// enrolled key. This provides no isolation from code under that account.
    /// Initial enrollment still requires an actual root/wheel bootstrap peer.
    pub fn same_account(uid: u32, gid: u32) -> Result<Self, PeerError> {
        if uid == 0 {
            return Err(PeerError::UnexpectedPrincipal);
        }
        Ok(Self {
            uid,
            gid,
            access: AccountAccess::SameAccount,
        })
    }

    pub(crate) fn permits_enrollment(&self, peer: &PeerIdentity) -> bool {
        match self.access {
            AccountAccess::Separate { .. } => self.matches_operator(peer),
            AccountAccess::SameAccount => peer.uid == 0 && peer.gid == 0,
        }
    }

    pub(crate) fn permits_assertion(&self, peer: &PeerIdentity) -> bool {
        self.matches_operator(peer)
    }

    fn matches_operator(&self, peer: &PeerIdentity) -> bool {
        peer.uid == self.uid && peer.gid == self.gid
    }
}

/// Get effective peer identity from the kernel, never from request data.
/// Apple documents these credentials as fixed at connect/listen time.
pub fn inspect_peer(stream: &UnixStream) -> Result<PeerIdentity, PeerError> {
    let (uid, gid) = getpeereid(stream).map_err(|_| PeerError::CredentialsUnavailable)?;
    Ok(PeerIdentity {
        uid: uid.as_raw(),
        gid: gid.as_raw(),
    })
}

/// Inspect the connected peer and enforce the selected account policy. In
/// same-account mode root/wheel can only bootstrap enrollment; callers must
/// still enforce the per-ceremony permission. Identity alone is not approval.
pub fn check_operator_peer(
    stream: &UnixStream,
    expected: &ExpectedOperator,
) -> Result<PeerIdentity, PeerError> {
    let peer = inspect_peer(stream)?;
    match expected.access {
        AccountAccess::Separate { agent_uid } if peer.uid == agent_uid => {
            return Err(PeerError::SharedPrincipal);
        }
        AccountAccess::SameAccount if peer.uid == 0 && peer.gid == 0 => return Ok(peer),
        _ => (),
    }
    if !expected.matches_operator(&peer) {
        return Err(PeerError::UnexpectedPrincipal);
    }
    Ok(peer)
}

#[cfg(test)]
mod tests {
    use super::*;
    use nix::unistd::{getegid, geteuid};
    use std::os::unix::net::UnixListener;

    fn connected() -> (tempfile::TempDir, UnixStream, UnixStream) {
        // A short pathname avoids macOS's bounded sockaddr_un path length.
        let directory = tempfile::Builder::new()
            .prefix("task153-peer-")
            .tempdir_in("/private/tmp")
            .unwrap();
        let path = directory.path().join("socket");
        let listener = UnixListener::bind(&path).unwrap();
        let client = UnixStream::connect(path).unwrap();
        let (server, _) = listener.accept().unwrap();
        (directory, client, server)
    }

    #[test]
    fn actual_local_socket_reports_kernel_peer_uid_and_gid_to_both_sides() {
        let (_directory, client, server) = connected();
        for stream in [&client, &server] {
            let peer = inspect_peer(stream).unwrap();
            assert_eq!(peer.uid(), geteuid().as_raw());
            assert_eq!(peer.gid(), getegid().as_raw());
        }
    }

    #[test]
    fn shared_agent_principal_is_rejected_even_when_socket_identity_is_real() {
        let (_directory, _client, server) = connected();
        let uid = geteuid().as_raw();
        let other = uid.checked_add(1).unwrap();
        let expected = ExpectedOperator::new(other, getegid().as_raw(), uid).unwrap();
        assert_eq!(
            check_operator_peer(&server, &expected),
            Err(PeerError::SharedPrincipal)
        );
        assert!(matches!(
            ExpectedOperator::new(uid, getegid().as_raw(), uid),
            Err(PeerError::SharedPrincipal)
        ));
    }

    #[test]
    fn incorrect_expected_uid_or_gid_is_rejected_without_any_approval() {
        let (_directory, _client, server) = connected();
        let uid = geteuid().as_raw();
        let gid = getegid().as_raw();
        // These deliberately wrong policy IDs test matching only. They do not
        // prove that a separately provisioned principal exists on this host.
        let unrelated = uid.checked_add(2).unwrap();
        for expected in [
            ExpectedOperator::new(uid.checked_add(1).unwrap(), gid, unrelated).unwrap(),
            ExpectedOperator::new(uid, gid.checked_add(1).unwrap(), unrelated).unwrap(),
        ] {
            assert_eq!(
                check_operator_peer(&server, &expected),
                Err(PeerError::UnexpectedPrincipal)
            );
        }
    }

    #[test]
    fn same_account_bootstrap_policy_requires_exact_root_wheel_peer() {
        let policy = ExpectedOperator::same_account(41001, 41002).unwrap();
        let bootstrap = PeerIdentity { uid: 0, gid: 0 };
        assert!(policy.permits_enrollment(&bootstrap));
        assert!(!policy.permits_assertion(&bootstrap));
        for peer in [
            PeerIdentity {
                uid: 41001,
                gid: 41002,
            },
            PeerIdentity { uid: 0, gid: 41002 },
            PeerIdentity { uid: 41003, gid: 0 },
        ] {
            assert!(!policy.permits_enrollment(&peer));
        }
        assert!(policy.permits_assertion(&PeerIdentity {
            uid: 41001,
            gid: 41002
        }));
        assert!(!policy.permits_assertion(&PeerIdentity {
            uid: 41001,
            gid: 41003
        }));
        assert!(ExpectedOperator::same_account(0, 0).is_err());
    }
}
