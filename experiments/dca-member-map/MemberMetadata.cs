// Original Task-142 tooling. Bounded selected grammar using public Metadata APIs.
// Primary grammar: ECMA-335 VI, II.23.2; .NET runtime v10.0.0 SignatureDecoder.cs.
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

namespace Task142 {
public static class MemberMetadata {
    const int BlobLimit = 512, RowLimit = 32768, DepthLimit = 16, NodeLimit = 128;
    const int ReportLimit = 2 * 1024 * 1024, WorkLimit = 1000000;
    sealed class ObservationException : Exception { public ObservationException(string code) : base(code) {} }
    static Exception Fail(string code) { return new InvalidDataException("DCAMEMBER_" + code); }
    static void Unsupported(string code) { throw new ObservationException("UNSUPPORTED_" + code); }
    static Dictionary<string, object> Obj(params object[] pairs) {
        var result = new Dictionary<string, object>();
        for (int i = 0; i < pairs.Length; i += 2) result.Add((string)pairs[i], pairs[i+1]);
        return result;
    }
    static string Hex(byte[] b) { return Convert.ToHexString(b).ToLowerInvariant(); }
    static string Hash(byte[] b) { return Hex(SHA256.HashData(b)); }
    static string Str(MetadataReader r, StringHandle h) {
        string s = r.GetString(h); if (s.Length > 1024) throw Fail("STRING_LIMIT"); return s;
    }
    sealed class Identity {
        public string Name, Version, Culture, Token;
        public object Report() { return Obj("name",Name,"version",Version,"culture",Culture,"public_key_token",Token); }
        public string Key(bool mapped) { return JsonSerializer.Serialize(new string[]{Name,mapped ? "candidate-version-observation" : Version,Culture,Token}); }
    }
    static string Token(MetadataReader r, BlobHandle h, bool full) {
        var b = r.GetBlobReader(h);
        if (b.Length == 0) return "null";
        if (b.Length > 16384 || (!full && b.Length != 8)) throw Fail("PUBLIC_KEY_SHAPE");
        byte[] bytes = b.ReadBytes(b.Length);
        if (!full) return Hex(bytes);
        byte[] hash = SHA1.HashData(bytes), token = new byte[8];
        for(int i=0;i<8;i++) token[i]=hash[hash.Length-1-i];
        return Hex(token);
    }
    static Identity DefinitionIdentity(MetadataReader r) {
        var a=r.GetAssemblyDefinition(); return new Identity { Name=Str(r,a.Name),Version=a.Version.ToString(),Culture=a.Culture.IsNil?"neutral":Str(r,a.Culture),Token=Token(r,a.PublicKey,true) };
    }
    static Identity ReferenceIdentity(MetadataReader r, AssemblyReferenceHandle h) {
        var a=r.GetAssemblyReference(h); return new Identity { Name=Str(r,a.Name),Version=a.Version.ToString(),Culture=a.Culture.IsNil?"neutral":Str(r,a.Culture),Token=Token(r,a.PublicKeyOrToken,(a.Flags & AssemblyFlags.PublicKey)!=0) };
    }
    sealed class Input : IDisposable {
        public string Filename, Hash; public byte[] Bytes; public PEReader Pe; public MetadataReader R; public Identity Identity;
        public Dictionary<string,TypeDefinitionHandle> Types=new Dictionary<string,TypeDefinitionHandle>(StringComparer.Ordinal);
        public void Dispose() { if(Pe!=null) Pe.Dispose(); }
    }
    sealed class Scope { public string Name; public Identity Assembly; }
    sealed class Node { public string Shape; public Scope Owner; public Node(string shape, Scope owner=null) { Shape=shape; Owner=owner; } }
    sealed class Context {
        public HashSet<string> Mappings; public int Work; int ReportBytes;
        public void Tick() { if(++Work>WorkLimit) throw Fail("WORK_LIMIT"); }
        // Conservative incremental budget prevents collecting oversized reports.
        public T Charge<T>(T value) { ReportBytes += Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(value)); if(ReportBytes>ReportLimit) throw Fail("REPORT_LIMIT"); return value; }
        public string Key(Identity identity) { return identity.Key(Mappings.Contains(identity.Name)); }
    }
    static void TypeIdentifier(string name,string ns,bool nested) {
        // Selected profile: keep display-name flattening injective, or reject input.
        if(name.IndexOf('.')>=0 || name.IndexOf('+')>=0 || ns.IndexOf('+')>=0 || (nested && ns.Length!=0))
            throw Fail("UNSUPPORTED_TYPE_IDENTIFIER");
    }
    static string TypeDefName(Input input, TypeDefinitionHandle h, int depth=0) {
        if(depth>=DepthLimit) throw Fail("TYPE_SCOPE_LIMIT");
        var t=input.R.GetTypeDefinition(h); string name=Str(input.R,t.Name), ns=Str(input.R,t.Namespace);
        var parent=t.GetDeclaringType();
        TypeIdentifier(name,ns,!parent.IsNil);
        string result=parent.IsNil?(ns.Length==0?name:ns+"."+name):TypeDefName(input,parent,depth+1)+"+"+name;
        if(result.Length>1024) throw Fail("STRING_LIMIT"); return result;
    }
    static Scope TypeRefScope(Input input, TypeReferenceHandle h, int depth=0) {
        if(depth>=DepthLimit) throw Fail("TYPE_SCOPE_LIMIT");
        var t=input.R.GetTypeReference(h); string name=Str(input.R,t.Name), ns=Str(input.R,t.Namespace);
        Scope result;
        TypeIdentifier(name,ns,t.ResolutionScope.Kind==HandleKind.TypeReference);
        if(t.ResolutionScope.Kind==HandleKind.AssemblyReference) result=new Scope { Name=ns.Length==0?name:ns+"."+name, Assembly=ReferenceIdentity(input.R,(AssemblyReferenceHandle)t.ResolutionScope) };
        else if(t.ResolutionScope.Kind==HandleKind.TypeReference) { result=TypeRefScope(input,(TypeReferenceHandle)t.ResolutionScope,depth+1); result.Name+="+"+name; }
        else { Unsupported("TYPE_SCOPE_"+t.ResolutionScope.Kind); return null; }
        if(result.Name.Length>1024) throw Fail("STRING_LIMIT"); return result;
    }
    sealed class Decoder {
        readonly Input Input; readonly Context Context; int Nodes; readonly HashSet<TypeSpecificationHandle> Active;
        public Decoder(Input input, Context context, HashSet<TypeSpecificationHandle> active=null) { Input=input;Context=context;Active=active??new HashSet<TypeSpecificationHandle>(); }
        public BlobReader Blob(BlobHandle h) { var b=Input.R.GetBlobReader(h); if(b.Length>BlobLimit) throw Fail("BLOB_LIMIT"); return b; }
        int Count(ref BlobReader b) { int count=b.ReadCompressedInteger(); if(count>NodeLimit) throw Fail("COUNT_LIMIT"); return count; }
        void Tick(int depth) { Context.Tick(); if(depth>=DepthLimit) throw Fail("DEPTH_LIMIT"); if(++Nodes>NodeLimit) throw Fail("NODE_LIMIT"); }
        Node Named(EntityHandle h, byte kind) {
            if(h.IsNil) throw new BadImageFormatException();
            Scope scope;
            if(h.Kind==HandleKind.TypeReference) scope=TypeRefScope(Input,(TypeReferenceHandle)h);
            else if(h.Kind==HandleKind.TypeDefinition) scope=new Scope { Name=TypeDefName(Input,(TypeDefinitionHandle)h), Assembly=Input.Identity };
            else { Unsupported("NAMED_TYPE_"+h.Kind); return null; }
            return new Node("named("+kind+","+JsonSerializer.Serialize(Context.Key(scope.Assembly))+","+JsonSerializer.Serialize(scope.Name)+")",scope);
        }
        public Node Type(ref BlobReader b,int depth=0,bool allowVoid=false) {
            Tick(depth); int start=b.Offset; var code=b.ReadSignatureTypeCode();
            switch(code) {
            case SignatureTypeCode.TypeHandle:
                // ReadSignatureTypeCode collapses CLASS/VALUETYPE; preserve the raw kind.
                b.Offset=start; byte kind=(byte)b.ReadCompressedInteger();
                return Named(b.ReadTypeHandle(),kind);
            case SignatureTypeCode.GenericTypeParameter: return new Node("!"+Count(ref b));
            case SignatureTypeCode.GenericMethodParameter: return new Node("!!"+Count(ref b));
            case SignatureTypeCode.SZArray: return new Node("sz("+Type(ref b,depth+1).Shape+")");
            case SignatureTypeCode.Pointer: return new Node("ptr("+Type(ref b,depth+1,true).Shape+")");
            case SignatureTypeCode.ByReference: return new Node("ref("+Type(ref b,depth+1).Shape+")");
            case SignatureTypeCode.RequiredModifier:
            case SignatureTypeCode.OptionalModifier:
                var modifier=Named(b.ReadTypeHandle(),0);
                return new Node(code+"("+modifier.Shape+","+Type(ref b,depth+1,allowVoid).Shape+")");
            case SignatureTypeCode.GenericTypeInstance:
                int genericKind=b.ReadCompressedInteger();
                if(genericKind!=(int)SignatureTypeKind.Class && genericKind!=(int)SignatureTypeKind.ValueType) throw new BadImageFormatException();
                Tick(depth+1); var generic=Named(b.ReadTypeHandle(),(byte)genericKind);
                int count=Count(ref b); if(count==0) throw new BadImageFormatException();
                var parts=new List<string>();
                for(int i=0;i<count;i++) parts.Add(Type(ref b,depth+1).Shape);
                return new Node("generic("+generic.Shape+";"+string.Join(";",parts)+")",generic.Owner);
            case SignatureTypeCode.Void:
                if(!allowVoid) throw new BadImageFormatException(); return new Node("Void");
            case SignatureTypeCode.Boolean: case SignatureTypeCode.Char:
            case SignatureTypeCode.SByte: case SignatureTypeCode.Byte:
            case SignatureTypeCode.Int16: case SignatureTypeCode.UInt16:
            case SignatureTypeCode.Int32: case SignatureTypeCode.UInt32:
            case SignatureTypeCode.Int64: case SignatureTypeCode.UInt64:
            case SignatureTypeCode.Single: case SignatureTypeCode.Double:
            case SignatureTypeCode.IntPtr: case SignatureTypeCode.UIntPtr:
            case SignatureTypeCode.Object: case SignatureTypeCode.String:
            case SignatureTypeCode.TypedReference: return new Node(code.ToString());
            case SignatureTypeCode.Array: case SignatureTypeCode.FunctionPointer:
            case SignatureTypeCode.Pinned: case SignatureTypeCode.Sentinel:
                Unsupported("TYPE_"+code); return null;
            default: throw new BadImageFormatException();
            }
        }
        public string Member(BlobHandle h) {
            var b=Blob(h); var header=b.ReadSignatureHeader(); string result;
            if(header.Kind==SignatureKind.Field && header.RawValue==6) result="field:"+Type(ref b).Shape;
            else if(header.Kind==SignatureKind.Method) {
                if((header.RawValue & 0x80)!=0) throw new BadImageFormatException();
                if(header.CallingConvention!=SignatureCallingConvention.Default) { Unsupported("CALLING_CONVENTION_"+header.CallingConvention); }
                int generics=header.IsGeneric?Count(ref b):0;
                int count=Count(ref b); string returns=Type(ref b,0,true).Shape;
                var args=new List<string>(); for(int i=0;i<count;i++) args.Add(Type(ref b).Shape);
                result="method:"+header.RawValue+":"+generics+":"+returns+"("+string.Join(";",args)+")";
            } else throw new BadImageFormatException();
            if(b.RemainingBytes!=0) throw new BadImageFormatException(); return result;
        }
        public Node Spec(TypeSpecificationHandle h) {
            if(!Active.Add(h)) throw Fail("TYPESPEC_CYCLE");
            try { var b=Blob(Input.R.GetTypeSpecification(h).Signature); var node=Type(ref b); if(b.RemainingBytes!=0) throw new BadImageFormatException(); return node; }
            finally { Active.Remove(h); }
        }
        public Node Parent(EntityHandle h) {
            if(h.Kind==HandleKind.TypeReference) return Named(h,0);
            if(h.Kind==HandleKind.TypeSpecification) return Spec((TypeSpecificationHandle)h);
            Unsupported("MEMBER_PARENT_"+h.Kind); return null;
        }
    }
    static byte[] ReadPinned(string path,string hash,ref long total) {
        if(hash==null||hash.Length!=64) throw Fail("EXPECTED_HASH_FORMAT");
        foreach(char c in hash) if(!Uri.IsHexDigit(c)) throw Fail("EXPECTED_HASH_FORMAT");
        if(!Path.IsPathFullyQualified(path)||path.StartsWith("//")||path.StartsWith("\\\\")) throw Fail("LOCAL_PATH_REQUIRED");
        string current=Path.GetFullPath(path);
        while(current!=null) { if((File.GetAttributes(current)&FileAttributes.ReparsePoint)!=0) throw Fail("LINK_PATH"); current=Path.GetDirectoryName(current); }
        using(var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read)) {
            long size=stream.Length; if(size<=0||size>16*1024*1024) throw Fail("FILE_LIMIT");
            total+=size; if(total>32*1024*1024) throw Fail("TOTAL_LIMIT");
            byte[] bytes=new byte[(int)size]; stream.ReadExactly(bytes);
            if(stream.ReadByte()!=-1||stream.Length!=size) throw Fail("SOURCE_CHANGED");
            if(!string.Equals(Hash(bytes),hash,StringComparison.OrdinalIgnoreCase)) throw Fail("HASH_MISMATCH"); return bytes;
        }
    }
    static void Parse(Input input) {
        input.Pe=new PEReader(new MemoryStream(input.Bytes,false));
        if(!input.Pe.HasMetadata) throw Fail("NO_MANAGED_METADATA");
        input.R=input.Pe.GetMetadataReader(MetadataReaderOptions.None);
        if(!input.R.IsAssembly) throw Fail("NOT_ASSEMBLY");
        foreach(TableIndex table in Enum.GetValues<TableIndex>()) if(input.R.GetTableRowCount(table)>RowLimit) throw Fail("ROW_LIMIT");
        input.Identity=DefinitionIdentity(input.R);
        foreach(var h in input.R.TypeDefinitions) if(!input.Types.TryAdd(TypeDefName(input,h),h)) throw Fail("DUPLICATE_TYPE_NAME");
    }
    static List<object> FrameworkAttributes(Input input,Context context) {
        var result=new List<object>();
        foreach(var h in input.R.GetAssemblyDefinition().GetCustomAttributes()) {
            context.Tick(); var attr=input.R.GetCustomAttribute(h); Scope scope=null; string ctor=null;
            if(attr.Constructor.Kind==HandleKind.MemberReference) {
                var member=input.R.GetMemberReference((MemberReferenceHandle)attr.Constructor); ctor=Str(input.R,member.Name);
                if(member.Parent.Kind==HandleKind.TypeReference) {
                    try { scope=TypeRefScope(input,(TypeReferenceHandle)member.Parent); } catch(ObservationException) { }
                }
            }
            if(scope==null||scope.Name!="System.Runtime.Versioning.TargetFrameworkAttribute"||ctor!=".ctor") continue;
            var blob=input.R.GetBlobReader(attr.Value); if(blob.Length>BlobLimit) throw Fail("ATTRIBUTE_BLOB_LIMIT");
            byte[] bytes=blob.ReadBytes(blob.Length);
            result.Add(context.Charge(Obj("attribute_row",MetadataTokens.GetRowNumber(h),"constructor_assembly",scope.Assembly.Report(),"value_bytes",bytes.Length,"value_hex",Hex(bytes),"value_sha256",Hash(bytes),"interpretation","RAW_METADATA_ONLY")));
        }
        return result;
    }
    static string DecodeMember(Input input,Context context,BlobHandle blob,out string error) {
        try { error=null; return new Decoder(input,context).Member(blob); }
        catch(ObservationException e) { error=e.Message; return null; }
        catch(BadImageFormatException) { error="MALFORMED_SIGNATURE"; return null; }
    }
    static Dictionary<string,object> Compare(Input dca,Context context,MemberReferenceHandle h,Node parent,Input candidate) {
        var member=dca.R.GetMemberReference(h); string name=Str(dca.R,member.Name);
        string error; string requested=DecodeMember(dca,context,member.Signature,out error);
        var matches=new List<object>(); var alternatives=new List<object>(); bool unsupported=false;
        TypeDefinitionHandle type; bool present=candidate.Types.TryGetValue(parent.Owner.Name,out type);
        // Version is the only permitted owner-identity observation difference.
        bool ownerIdentityMatch=parent.Owner.Assembly.Key(true)==candidate.Identity.Key(true);
        if(present && requested!=null && ownerIdentityMatch) {
            var definition=candidate.R.GetTypeDefinition(type);
            if(member.GetKind()==MemberReferenceKind.Field) {
                foreach(var fieldHandle in definition.GetFields()) {
                    context.Tick(); var field=candidate.R.GetFieldDefinition(fieldHandle);
                    if(Str(candidate.R,field.Name)!=name) continue;
                    string e; string shape=DecodeMember(candidate,context,field.Signature,out e);
                    alternatives.Add(context.Charge(Obj("row",MetadataTokens.GetRowNumber(fieldHandle),"kind","field","shape",shape,"error",e)));
                    if(shape==requested) matches.Add(Obj("row",MetadataTokens.GetRowNumber(fieldHandle),"kind","field"));
                    if(e!=null) unsupported=true;
                }
            } else {
                foreach(var methodHandle in definition.GetMethods()) {
                    context.Tick(); var method=candidate.R.GetMethodDefinition(methodHandle);
                    if(Str(candidate.R,method.Name)!=name) continue;
                    string e; string shape=DecodeMember(candidate,context,method.Signature,out e);
                    alternatives.Add(context.Charge(Obj("row",MetadataTokens.GetRowNumber(methodHandle),"kind","method","shape",shape,"error",e)));
                    if(shape==requested) matches.Add(Obj("row",MetadataTokens.GetRowNumber(methodHandle),"kind","method"));
                    if(e!=null) unsupported=true;
                }
            }
        }
        string disposition=error??(!ownerIdentityMatch?"OWNER_IDENTITY_MISMATCH_UNRESOLVED":(matches.Count>0?"DIRECT_SIGNATURE_STRUCTURAL_MATCH":(!present?"TYPEDEF_NOT_FOUND_FORWARDING_UNRESOLVED":(unsupported?"CANDIDATE_SIGNATURE_UNRESOLVED":(alternatives.Count==0?"NO_DIRECT_NAMED_DEFINITION_INHERITANCE_UNRESOLVED":"NO_DIRECT_SIGNATURE_MATCH_INHERITANCE_UNRESOLVED")))));
        return Obj("row",MetadataTokens.GetRowNumber(h),"parent_kind",member.Parent.Kind.ToString(),"declaring_type",parent.Owner.Name,"parent_shape",parent.Shape,
            "member_name",name,"requested_assembly",parent.Owner.Assembly.Report(),"supplied_assembly",candidate.Identity.Report(),
            "exact_assembly_identity_match",parent.Owner.Assembly.Key(false)==candidate.Identity.Key(false),"requested_signature",requested,
            "disposition",disposition,"direct_matches",matches,"direct_same_name_definitions",alternatives);
    }
    public static string Read(string dcaPath,string dcaHash,string[] references,string[] hashes) {
        if(references==null||hashes==null||references.Length<1||references.Length>2||references.Length!=hashes.Length) throw Fail("INPUT_COUNT");
        var inputs=new List<Input>();
        try {
            long total=0; var paths=new List<string>{dcaPath};paths.AddRange(references);var pins=new List<string>{dcaHash};pins.AddRange(hashes);
            var unique=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for(int i=0;i<paths.Count;i++) {
                if(!unique.Add(Path.GetFullPath(paths[i]))) throw Fail("DUPLICATE_PATH");
                byte[] bytes=ReadPinned(paths[i],pins[i],ref total);
                inputs.Add(new Input { Filename=Path.GetFileName(paths[i]),Bytes=bytes,Hash=Hash(bytes) });
            }
            foreach(var input in inputs) Parse(input);
            var candidates=new Dictionary<string,Input>(StringComparer.Ordinal);
            for(int i=1;i<inputs.Count;i++) if(!candidates.TryAdd(inputs[i].Identity.Name,inputs[i])) throw Fail("DUPLICATE_ASSEMBLY_NAME");
            var context=new Context { Mappings=new HashSet<string>(candidates.Keys,StringComparer.Ordinal) };
            var inputReports=new List<object>();
            foreach(var input in inputs) inputReports.Add(context.Charge(Obj("filename",input.Filename,"bytes",input.Bytes.Length,"sha256",input.Hash,"identity",input.Identity.Report(),"target_framework_attributes",FrameworkAttributes(input,context))));
            var members=new List<object>(); var specs=new List<object>(); var unsupported=new List<object>(); int directCount=0;
            var dca=inputs[0];
            foreach(var h in dca.R.MemberReferences) {
                context.Tick(); var member=dca.R.GetMemberReference(h); Node parent;
                try { parent=new Decoder(dca,context).Parent(member.Parent); }
                catch(ObservationException e) { unsupported.Add(context.Charge(Obj("row",MetadataTokens.GetRowNumber(h),"parent_kind",member.Parent.Kind.ToString(),"reason",e.Message))); continue; }
                catch(BadImageFormatException) { unsupported.Add(context.Charge(Obj("row",MetadataTokens.GetRowNumber(h),"parent_kind",member.Parent.Kind.ToString(),"reason","MALFORMED_PARENT_SIGNATURE"))); continue; }
                bool supplied=parent.Owner!=null&&candidates.ContainsKey(parent.Owner.Assembly.Name);
                if(member.Parent.Kind==HandleKind.TypeSpecification) specs.Add(context.Charge(Obj("member_row",MetadataTokens.GetRowNumber(h),"typespec_row",MetadataTokens.GetRowNumber(member.Parent),"shape",parent.Shape,"owner_type",parent.Owner?.Name,"owner_assembly",parent.Owner?.Assembly.Report(),"candidate_directed",supplied,"disposition",parent.Owner==null?"DECODED_NON_NAMED_OWNER_UNSUPPORTED":"DECODED_OWNER_SCOPE")));
                if(!supplied) continue;
                if(member.Parent.Kind==HandleKind.TypeReference) directCount++;
                members.Add(context.Charge(Compare(dca,context,h,parent,candidates[parent.Owner.Assembly.Name])));
            }
            var result=Obj("schema_version",1,"operation","static_candidate_member_map","rights","UNRESOLVED","runtime_execution","NOT_RUN",
                "inputs",inputReports,"mapping_rule","Candidate assembly names only: compare named types with name/culture/token; candidate versions omitted from structural shape only. Other assembly identities include version. No binding redirect.",
                "direct_typeref_candidate_members",directCount,"candidate_members",members,"typespec_member_parents",specs,"unresolved_member_parents",unsupported,
                "limits",Obj("file_bytes",16*1024*1024,"total_bytes",32*1024*1024,"table_rows",RowLimit,"string_chars",1024,"blob_bytes",BlobLimit,"type_depth",DepthLimit,"nodes_per_signature",NodeLimit,"count",NodeLimit,"work_units",WorkLimit,"report_bytes",ReportLimit),
                "limitations",new string[]{"Direct signature structural comparison only; no ABI, binding, accessibility, generic constraint or implementation validation.","Inheritance, forwarding and external dependency resolution are not performed; nonmatches never prove absence.","Array shapes, function pointers, pinned types, sentinel and nondefault calling conventions are explicitly unsupported.","Framework attributes are raw bytes, not instantiated or executed. No method bodies, vendor loads, or bus operations."});
            return Serialize(result);
        } catch(BadImageFormatException) { throw Fail("MALFORMED_METADATA"); }
        finally { foreach(var input in inputs) input.Dispose(); }
    }
    static string Serialize(object result) { string json=JsonSerializer.Serialize(result); if(Encoding.UTF8.GetByteCount(json)>ReportLimit) throw Fail("REPORT_LIMIT");return json; }
    // Owned negative fixtures use this entry point with a synthetic managed file.
    public static string ProbeSignature(string path, int blobOffset, bool member) {
        if(new FileInfo(path).Length>16*1024*1024) throw Fail("FILE_LIMIT");
        byte[] bytes=File.ReadAllBytes(path);
        using(var pe=new PEReader(new MemoryStream(bytes,false))) {
            var input=new Input{Bytes=bytes,Pe=pe,R=pe.GetMetadataReader(MetadataReaderOptions.None)}; input.Identity=DefinitionIdentity(input.R);
            var context=new Context{Mappings=new HashSet<string>()}; var decoder=new Decoder(input,context);
            if(member) return decoder.Member(MetadataTokens.BlobHandle(blobOffset));
            var b=decoder.Blob(MetadataTokens.BlobHandle(blobOffset)); var node=decoder.Type(ref b); if(b.RemainingBytes!=0) throw new BadImageFormatException(); return node.Shape;
        }
    }
    public static void ProbeWorkLimit() { var context=new Context(); for(int i=0;i<=WorkLimit;i++) context.Tick(); }
    public static void ProbeIncrementalReportLimit() { var context=new Context(); string part=new string('x',ReportLimit/2); context.Charge(part); context.Charge(part); }
    public static void ProbeReportLimit() { Serialize(new string('x',ReportLimit)); }
}
}
