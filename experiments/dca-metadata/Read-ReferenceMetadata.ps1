#requires -Version 7.6
<#
.SYNOPSIS
Bounded static identity/reference observations for one DCA and up to two candidates.
.DESCRIPTION
Task-140 research only. Use PowerShell 7.6.7. Real files MUST be staged and read
under the coordinator's separately verified read-only/network-denied child policy
and deadline/output cap. PEReader is not designed to handle untrusted input safely:
https://learn.microsoft.com/en-us/dotnet/api/system.reflection.portableexecutable.pereader
This is not a production parser, sandbox, API reconstruction or compatibility test.
All expected SHA-256 values are verified before any PEReader is constructed. The
reader uses file bytes, not target assembly loading, attributes, IL or method bodies.
.PARAMETER DcaPath
Absolute local path of the staged primary DLL. No archives or discovery.
.PARAMETER DcaSha256
Expected SHA-256 of that exact primary DLL, not its containing archive.
.PARAMETER ReferencePath
One or two explicit staged candidate DLL paths, positional with ReferenceSha256.
.PARAMETER ReferenceSha256
Expected SHA-256 values for the corresponding ReferencePath entries.
.PARAMETER IncludeSignatureHex
Opt in to private raw MemberRef signature hex, maximum 512 bytes per signature.
Signature lengths and SHA-256 are always included; signatures are not decoded.
.OUTPUTS
One compact JSON report on stdout, no files. DCAMETA_* failures terminate nonzero.
TypeDef presence does not establish ABI/runtime compatibility; unresolved scopes,
forwarding, non-supplied dependencies and member signatures remain explicit.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string] $DcaPath,
    [Parameter(Mandatory)][string] $DcaSha256,
    [Parameter(Mandatory)][string[]] $ReferencePath,
    [Parameter(Mandatory)][string[]] $ReferenceSha256,
    [switch] $IncludeSignatureHex
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
try {
    if (-not ('Task140.StaticMetadata' -as [type])) {
        # Original tooling only. Roslyn compiles this helper in memory; no target
        # assembly is loaded or supplied to Add-Type as a reference.
        Add-Type -ReferencedAssemblies @('System.Reflection.Metadata', 'System.Collections.Immutable', 'System.Collections', 'System.Text.Json', 'System.Security.Cryptography', 'System.IO.FileSystem.DriveInfo') -TypeDefinition @'
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;

namespace Task140 {
public static class StaticMetadata {
    const int FileLimit = 16 * 1024 * 1024, TotalLimit = 32 * 1024 * 1024;
    const int RowLimit = 32768, StringLimit = 1024, SignatureLimit = 512;
    static Exception Fail(string code) { return new InvalidDataException("DCAMETA_" + code); }
    static Dictionary<string, object> Obj(params object[] pairs) {
        var result = new Dictionary<string, object>();
        for (int i = 0; i < pairs.Length; i += 2) result.Add((string)pairs[i], pairs[i + 1]);
        return result;
    }
    static string Hex(byte[] bytes) { return Convert.ToHexString(bytes).ToLowerInvariant(); }
    static string Hash(byte[] bytes) { return Hex(SHA256.HashData(bytes)); }
    static string Str(MetadataReader reader, StringHandle handle) {
        string text = reader.GetString(handle);
        if (text.Length > StringLimit) throw Fail("STRING_LIMIT");
        return text;
    }
    sealed class Identity {
        public string Name, Version, Culture, Token;
        public Dictionary<string, object> Report() { return Obj("name", Name, "version", Version, "culture", Culture, "public_key_token", Token); }
        public List<string> Differences(Identity other) {
            var result = new List<string>();
            if (Name != other.Name) result.Add("name");
            if (Version != other.Version) result.Add("version");
            if (Culture != other.Culture) result.Add("culture");
            if (Token != other.Token) result.Add("public_key_token");
            return result;
        }
    }
    static string Token(MetadataReader reader, BlobHandle handle, bool fullKey) {
        var blob = reader.GetBlobReader(handle);
        if (blob.Length == 0) return "null";
        if (blob.Length > 16384 || (!fullKey && blob.Length != 8)) throw Fail("PUBLIC_KEY_SHAPE");
        byte[] key = blob.ReadBytes(blob.Length);
        if (!fullKey) return Hex(key);
        byte[] hash = SHA1.HashData(key), token = new byte[8];
        for (int i = 0; i < 8; i++) token[i] = hash[hash.Length - 1 - i];
        return Hex(token);
    }
    static Identity DefinitionIdentity(MetadataReader r) {
        var a = r.GetAssemblyDefinition();
        return new Identity { Name = Str(r, a.Name), Version = a.Version.ToString(),
            Culture = a.Culture.IsNil ? "neutral" : Str(r, a.Culture), Token = Token(r, a.PublicKey, true) };
    }
    static Identity ReferenceIdentity(MetadataReader r, AssemblyReferenceHandle handle) {
        var a = r.GetAssemblyReference(handle);
        return new Identity { Name = Str(r, a.Name), Version = a.Version.ToString(),
            Culture = a.Culture.IsNil ? "neutral" : Str(r, a.Culture),
            Token = Token(r, a.PublicKeyOrToken, (a.Flags & AssemblyFlags.PublicKey) != 0) };
    }
    sealed class Input : IDisposable {
        public string Filename, Hash, Role;
        public byte[] Bytes;
        public PEReader Pe;
        public MetadataReader Reader;
        public Identity Identity;
        public Dictionary<string, TypeAttributes> Types = new Dictionary<string, TypeAttributes>(StringComparer.Ordinal);
        public List<Dictionary<string, object>> References = new List<Dictionary<string, object>>();
        public void Dispose() { if (Pe != null) Pe.Dispose(); }
    }
    static string TypeDefinitionName(MetadataReader r, TypeDefinitionHandle h, HashSet<TypeDefinitionHandle> seen) {
        if (seen.Count >= 16 || !seen.Add(h)) throw Fail("TYPE_SCOPE_LIMIT");
        var t = r.GetTypeDefinition(h);
        string name = Str(r, t.Name), ns = Str(r, t.Namespace);
        var parent = t.GetDeclaringType();
        string full = !parent.IsNil ? TypeDefinitionName(r, parent, seen) + "+" + name : (ns.Length == 0 ? name : ns + "." + name);
        if (full.Length > StringLimit) throw Fail("STRING_LIMIT");
        return full;
    }
    sealed class TypeScope {
        public string Name, Unsupported;
        public AssemblyReferenceHandle Assembly;
    }
    static TypeScope TypeReferenceScope(MetadataReader r, TypeReferenceHandle h, HashSet<TypeReferenceHandle> seen) {
        if (seen.Count >= 16 || !seen.Add(h)) throw Fail("TYPE_SCOPE_LIMIT");
        var t = r.GetTypeReference(h);
        string name = Str(r, t.Name), ns = Str(r, t.Namespace);
        var scope = t.ResolutionScope;
        TypeScope result;
        if (scope.Kind == HandleKind.AssemblyReference) result = new TypeScope { Name = ns.Length == 0 ? name : ns + "." + name, Assembly = (AssemblyReferenceHandle)scope };
        else if (scope.Kind == HandleKind.TypeReference) {
            result = TypeReferenceScope(r, (TypeReferenceHandle)scope, seen);
            result.Name += "+" + name;
        }
        else result = new TypeScope { Name = ns.Length == 0 ? name : ns + "." + name, Unsupported = scope.Kind.ToString() };
        if (result.Name.Length > StringLimit) throw Fail("STRING_LIMIT");
        return result;
    }
    static byte[] ReadPinned(string path, string expected, ref long total) {
        if (expected == null || expected.Length != 64) throw Fail("EXPECTED_HASH_FORMAT");
        foreach (char c in expected) if (!Uri.IsHexDigit(c)) throw Fail("EXPECTED_HASH_FORMAT");
        if (!Path.IsPathFullyQualified(path) || path.StartsWith("\\\\") || path.StartsWith("//")) throw Fail("LOCAL_PATH_REQUIRED");
        if (OperatingSystem.IsWindows()) {
            DriveType type = new DriveInfo(Path.GetPathRoot(path)).DriveType;
            if (type != DriveType.Fixed && type != DriveType.Removable && type != DriveType.Ram) throw Fail("LOCAL_PATH_REQUIRED");
        }
        string current = Path.GetFullPath(path);
        while (current != null) {
            if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0) throw Fail("LINK_PATH");
            current = Path.GetDirectoryName(current);
        }
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
            long size = stream.Length;
            if (size <= 0 || size > FileLimit) throw Fail("FILE_LIMIT");
            total += size;
            if (total > TotalLimit) throw Fail("TOTAL_LIMIT");
            byte[] bytes = new byte[(int)size];
            stream.ReadExactly(bytes);
            if (stream.ReadByte() != -1 || stream.Length != size) throw Fail("SOURCE_CHANGED");
            if (!string.Equals(Hash(bytes), expected, StringComparison.OrdinalIgnoreCase)) throw Fail("HASH_MISMATCH");
            return bytes;
        }
    }
    static void Parse(Input input) {
        input.Pe = new PEReader(new MemoryStream(input.Bytes, false));
        if (!input.Pe.HasMetadata) throw Fail("NO_MANAGED_METADATA");
        input.Reader = input.Pe.GetMetadataReader(MetadataReaderOptions.None);
        var r = input.Reader;
        if (!r.IsAssembly) throw Fail("NOT_ASSEMBLY");
        // Check every present metadata table before iterating any row collection.
        foreach (TableIndex table in Enum.GetValues<TableIndex>()) if (r.GetTableRowCount(table) > RowLimit) throw Fail("ROW_LIMIT");
        input.Identity = DefinitionIdentity(r);
        foreach (var h in r.TypeDefinitions) {
            string name = TypeDefinitionName(r, h, new HashSet<TypeDefinitionHandle>());
            if (!input.Types.TryAdd(name, r.GetTypeDefinition(h).Attributes)) throw Fail("DUPLICATE_TYPE_NAME");
        }
        foreach (var h in r.AssemblyReferences) input.References.Add(Obj("row", MetadataTokens.GetRowNumber(h), "identity", ReferenceIdentity(r, h).Report()));
    }
    public static string Read(string dcaPath, string dcaHash, string[] refs, string[] hashes, bool includeHex) {
        if (refs == null || hashes == null || refs.Length < 1 || refs.Length > 2 || refs.Length != hashes.Length) throw Fail("INPUT_COUNT");
        var inputs = new List<Input>();
        try {
            long total = 0;
            var unique = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] paths = new string[refs.Length + 1], expected = new string[refs.Length + 1];
            paths[0] = dcaPath; expected[0] = dcaHash;
            Array.Copy(refs, 0, paths, 1, refs.Length); Array.Copy(hashes, 0, expected, 1, hashes.Length);
            for (int i = 0; i < paths.Length; i++) {
                if (!unique.Add(Path.GetFullPath(paths[i]))) throw Fail("DUPLICATE_PATH");
                byte[] bytes = ReadPinned(paths[i], expected[i], ref total);
                inputs.Add(new Input { Filename = Path.GetFileName(paths[i]), Bytes = bytes, Hash = Hash(bytes), Role = i == 0 ? "dca" : "candidate" });
            }
            // All inputs are pinned in immutable private byte snapshots first.
            foreach (var input in inputs) Parse(input);
            var candidates = new Dictionary<string, Input>(StringComparer.Ordinal);
            for (int i = 1; i < inputs.Count; i++) if (!candidates.TryAdd(inputs[i].Identity.Name, inputs[i])) throw Fail("DUPLICATE_ASSEMBLY_NAME");
            var inputReports = new List<object>();
            var dependencyMap = new List<object>();
            foreach (var input in inputs) {
                inputReports.Add(Obj("role", input.Role, "filename", input.Filename, "bytes", input.Bytes.Length, "sha256", input.Hash,
                    "identity", input.Identity.Report(), "direct_assembly_references", input.References));
                foreach (var h in input.Reader.AssemblyReferences) {
                    Identity requested = ReferenceIdentity(input.Reader, h);
                    Input supplied;
                    bool found = candidates.TryGetValue(requested.Name, out supplied);
                    dependencyMap.Add(Obj("source_filename", input.Filename, "reference_row", MetadataTokens.GetRowNumber(h), "requested_identity", requested.Report(),
                        "candidate_filename", found ? supplied.Filename : null, "candidate_identity", found ? supplied.Identity.Report() : null,
                        "observation", found ? "candidate_supplied" : "not_supplied_no_resolution_attempted",
                        "exact_identity_match", found && requested.Differences(supplied.Identity).Count == 0,
                        "identity_differences", found ? requested.Differences(supplied.Identity) : null));
                }
            }
            var dca = inputs[0];
            var typeReports = new List<object>();
            var memberReports = new List<object>();
            var unsupportedTypes = new List<object>();
            var unsupportedMemberCounts = new SortedDictionary<string, int>(StringComparer.Ordinal);
            foreach (var h in dca.Reader.TypeReferences) {
                var scope = TypeReferenceScope(dca.Reader, h, new HashSet<TypeReferenceHandle>());
                if (scope.Unsupported != null) {
                    unsupportedTypes.Add(Obj("row", MetadataTokens.GetRowNumber(h), "scope_kind", scope.Unsupported, "observation", "UNSUPPORTED_SCOPE"));
                    continue;
                }
                Identity requested = ReferenceIdentity(dca.Reader, scope.Assembly);
                Input supplied;
                if (!candidates.TryGetValue(requested.Name, out supplied)) continue;
                TypeAttributes attrs;
                bool found = supplied.Types.TryGetValue(scope.Name, out attrs);
                typeReports.Add(Obj("row", MetadataTokens.GetRowNumber(h), "type_name", scope.Name, "requested_assembly", requested.Report(),
                    "candidate_filename", supplied.Filename, "exact_identity_match", requested.Differences(supplied.Identity).Count == 0,
                    "definition_observation", found ? "same_name_typedef_present" : "not_found_in_typedefs_forwarding_unresolved",
                    "definition_kind", found ? ((attrs & TypeAttributes.Interface) != 0 ? "interface" : "class") : null));
            }
            foreach (var h in dca.Reader.MemberReferences) {
                var m = dca.Reader.GetMemberReference(h);
                if (m.Parent.Kind != HandleKind.TypeReference) {
                    string kind = m.Parent.Kind.ToString();
                    if (!unsupportedMemberCounts.ContainsKey(kind)) unsupportedMemberCounts[kind] = 0;
                    unsupportedMemberCounts[kind]++;
                    continue;
                }
                var scope = TypeReferenceScope(dca.Reader, (TypeReferenceHandle)m.Parent, new HashSet<TypeReferenceHandle>());
                if (scope.Unsupported != null) {
                    string kind = "TypeReference/" + scope.Unsupported;
                    if (!unsupportedMemberCounts.ContainsKey(kind)) unsupportedMemberCounts[kind] = 0;
                    unsupportedMemberCounts[kind]++;
                    continue;
                }
                var requested = ReferenceIdentity(dca.Reader, scope.Assembly);
                if (!candidates.ContainsKey(requested.Name)) continue;
                var blob = dca.Reader.GetBlobReader(m.Signature);
                if (blob.Length > SignatureLimit) throw Fail("SIGNATURE_LIMIT");
                byte[] signature = blob.ReadBytes(blob.Length);
                var report = Obj("row", MetadataTokens.GetRowNumber(h), "declaring_type", scope.Name, "assembly_name", requested.Name,
                    "member_name", Str(dca.Reader, m.Name), "signature_bytes", signature.Length, "signature_sha256", Hash(signature),
                    "signature_interpretation", "UNRESOLVED_NOT_DECODED", "member_definition_match", "NOT_CHECKED");
                if (includeHex) report.Add("signature_hex", Hex(signature));
                memberReports.Add(report);
            }
            var result = Obj("schema_version", 1, "operation", "static_candidate_contract_map", "rights", "UNRESOLVED", "runtime_execution", "NOT_RUN",
                "inputs", inputReports, "dependency_matrix", dependencyMap, "dca_types_to_candidates", typeReports,
                "dca_members_to_candidates", memberReports, "unsupported_type_scopes", unsupportedTypes, "unsupported_member_parent_counts", unsupportedMemberCounts,
                "limitations", new string[] { "TypeDef name/kind presence is not ABI or runtime compatibility.", "Type forwarding is not resolved; absent TypeDefs are not proof of absence.", "TypeSpec and other unsupported member scopes may hide candidate-directed members.", "No signature, member-definition, attribute, method-body or dependency resolution is performed." });
            string json = JsonSerializer.Serialize(result);
            if (Encoding.UTF8.GetByteCount(json) > 2 * 1024 * 1024) throw Fail("OUTPUT_LIMIT");
            return json;
        }
        catch (BadImageFormatException) { throw Fail("MALFORMED_METADATA"); }
        finally { foreach (var input in inputs) input.Dispose(); }
    }
}
}
'@
    }
    [Task140.StaticMetadata]::Read($DcaPath, $DcaSha256, $ReferencePath, $ReferenceSha256, $IncludeSignatureHex.IsPresent)
}
catch {
    $message = $_.Exception.ToString()
    $code = 'DCAMETA_IO_OR_METADATA'
    if ($message -match 'DCAMETA_[A-Z_]+') { $code = $Matches[0] }
    throw [System.InvalidOperationException]::new($code + ': Static metadata observation failed; no compatibility conclusion.')
}
