﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.



[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Crypto.Ed25519SigningService._expandedPrivateKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Crypto.Ed25519SigningService._secretKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Crypto.ConfidentialAssets.CryptoHelper.I")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Crypto.HashCalculations.HashCalculationBase._hash")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Crypto.StealthSigningService._secretSpendKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Crypto.StealthSigningService._secretViewKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'AccountSigningService.AccountSigningService(IHashCalculationsRepository hashCalculationRepository, IIdentityKeyProvidersRegistry identityKeyProvidersRegistry)', validate parameter 'identityKeyProvidersRegistry' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.Ed25519SigningService.#ctor(O10.Core.HashCalculations.IHashCalculationsRepository,O10.Core.Identity.IIdentityKeyProvidersRegistry)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object, object)' could vary based on the current user's locale settings. Replace this call in 'AccountSigningService.Sign(IPacket, [object])' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.Ed25519SigningService.Sign(O10.Core.Models.IPacket,System.Object)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void AccountSigningService.Sign(IPacket packet, object args = null)', validate parameter 'packet' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.Ed25519SigningService.Sign(O10.Core.Models.IPacket,System.Object)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object, object)' could vary based on the current user's locale settings. Replace this call in 'AccountSigningService.Verify(IPacket)' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.Ed25519SigningService.Verify(O10.Core.Models.IPacket)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'BorromeanRingSignatureEx ConfidentialAssetsHelper.CreateBorromeanRingSignature(byte[] msg, GroupElementP3[][] pubkeys, byte[][] privkeys, int[] indicies)' passes a literal string as parameter 'message' of a call to 'ArgumentOutOfRangeException.ArgumentOutOfRangeException(string paramName, string message)'. Retrieve the following string(s) from a resource table instead: \"number of signatures per ring cannot be less than 1\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.CreateBorromeanRingSignature(System.Byte[],Chaos.NaCl.Internal.Ed25519Ref10.GroupElementP3[][],System.Byte[][],System.Int32[])~O10.Core.Cryptography.BorromeanRingSignatureEx")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'EcdhTupleCA ConfidentialAssetsHelper.CreateEcdhTupleCA(byte[] blindingFactor, byte[] assetId, byte[] secretKey, byte[] receiverViewKey)', validate parameter 'blindingFactor' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.CreateEcdhTupleCA(System.Byte[],System.Byte[],System.Byte[],System.Byte[])~O10.Core.Cryptography.EcdhTupleCA")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'EcdhTupleProofs ConfidentialAssetsHelper.CreateEcdhTupleProofs(byte[] blindingFactor, byte[] assetId, byte[] issuer, byte[] payload, byte[] secretKey, byte[] receiverViewKey)', validate parameter 'issuer' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.CreateEcdhTupleProofs(System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[],System.Byte[])~O10.Core.Cryptography.EcdhTupleProofs")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'byte[] ConfidentialAssetsHelper.CreateEncodedCommitment(byte[] commitment, byte[] secretKey, byte[] receiverPublicKey)', validate parameter 'commitment' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.CreateEncodedCommitment(System.Byte[],System.Byte[],System.Byte[])~System.Byte[]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'BorromeanRingSignature ConfidentialAssetsHelper.CreateIssuanceSurjectionProof(GroupElementP3 assetCommitment, byte[] c, byte[][] assetIds, GroupElementP3[] issuanceKeys, int index, byte[] issuancePrivateKey)' passes a literal string as parameter 'message' of a call to 'ArgumentOutOfRangeException.ArgumentOutOfRangeException(string paramName, string message)'. Retrieve the following string(s) from a resource table instead: \"list of non-blinded asset IDs is empty\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.CreateIssuanceSurjectionProof(Chaos.NaCl.Internal.Ed25519Ref10.GroupElementP3,System.Byte[],System.Byte[][],Chaos.NaCl.Internal.Ed25519Ref10.GroupElementP3[],System.Int32,System.Byte[])~O10.Core.Cryptography.BorromeanRingSignature")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'SurjectionProof ConfidentialAssetsHelper.CreateNewIssuanceSurjectionProof(byte[] assetCommitment, byte[][] assetIds, int index, byte[] blindingFactor)', validate parameter 'assetIds' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.CreateNewIssuanceSurjectionProof(System.Byte[],System.Byte[][],System.Int32,System.Byte[])~O10.Core.Cryptography.SurjectionProof")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'SurjectionProof ConfidentialAssetsHelper.CreateSurjectionProof(byte[] assetCommitment, byte[][] candidateAssetCommitments, int index, byte[] blindingFactor, params byte[][] aux)', validate parameter 'candidateAssetCommitments' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.CreateSurjectionProof(System.Byte[],System.Byte[][],System.Int32,System.Byte[],System.Byte[][])~O10.Core.Cryptography.SurjectionProof")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'byte[] ConfidentialAssetsHelper.DecodeCommitment(byte[] encodedCommitment, byte[] transactionKey, byte[] secretKey)', validate parameter 'encodedCommitment' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.DecodeCommitment(System.Byte[],System.Byte[],System.Byte[])~System.Byte[]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void ConfidentialAssetsHelper.DecodeEcdhTuple(EcdhTupleCA ecdhTuple, byte[] transactionKey, byte[] secretKey, out byte[] blindingFactor, out byte[] assetId)', validate parameter 'ecdhTuple' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.DecodeEcdhTuple(O10.Core.Cryptography.EcdhTupleCA,System.Byte[],System.Byte[],System.Byte[]@,System.Byte[]@)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void ConfidentialAssetsHelper.DecodeEcdhTuple(EcdhTupleProofs ecdhTuple, byte[] transactionKey, byte[] secretKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload)', validate parameter 'ecdhTuple' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.DecodeEcdhTuple(O10.Core.Cryptography.EcdhTupleProofs,System.Byte[],System.Byte[],System.Byte[]@,System.Byte[]@,System.Byte[]@,System.Byte[]@)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'BorromeanRingSignatureEx ConfidentialAssetsHelper.GenBorromean(byte[][] x, byte[][] P1, byte[][] P2, byte[] indices)', validate parameter 'x' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.GenBorromean(System.Byte[][],System.Byte[][],System.Byte[][],System.Byte[])~O10.Core.Cryptography.BorromeanRingSignatureEx")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'BorromeanRingSignature ConfidentialAssetsHelper.GenerateBorromeanRingSignature(byte[] msg, byte[][] pks, int j, byte[] sk)', validate parameter 'pks' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.GenerateBorromeanRingSignature(System.Byte[],System.Byte[][],System.Int32,System.Byte[])~O10.Core.Cryptography.BorromeanRingSignature")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'BorromeanRingSignatureEx ConfidentialAssetsHelper.GenerateBorromeanRingSignature(byte[] msg, byte[][][] pubkeys, byte[][] privkeys, int[] indicies)', validate parameter 'indicies' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.GenerateBorromeanRingSignature(System.Byte[],System.Byte[][][],System.Byte[][],System.Int32[])~O10.Core.Cryptography.BorromeanRingSignatureEx")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'RingSignature[] ConfidentialAssetsHelper.GenerateRingSignature(byte[] msg, byte[] keyImage, byte[][] publicKeys, byte[] secretKey, int secretKeyIndex)', validate parameter 'publicKeys' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.GenerateRingSignature(System.Byte[],System.Byte[],System.Byte[][],System.Byte[],System.Int32)~O10.Core.Cryptography.RingSignature[]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'byte[] ConfidentialAssetsHelper.GetAssetIdFromEcdhTupleCA(EcdhTupleCA ecdhTuple, byte[] transactionKey, byte[] secretKey)', validate parameter 'ecdhTuple' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.GetAssetIdFromEcdhTupleCA(O10.Core.Cryptography.EcdhTupleCA,System.Byte[],System.Byte[])~System.Byte[]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'GroupElementP3 ConfidentialAssetsHelper.GetPublicKeyP3(byte[] secretKey)' passes a literal string as parameter 'message' of a call to 'ArgumentOutOfRangeException.ArgumentOutOfRangeException(string paramName, string message)'. Retrieve the following string(s) from a resource table instead: \"secretKey must be 32 bytes length\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.GetPublicKeyP3(System.Byte[])~Chaos.NaCl.Internal.Ed25519Ref10.GroupElementP3")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2000:Call System.IDisposable.Dispose on object created by 'RNGCryptoServiceProvider.Create()' before all references to it are out of scope.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.GetRandomSeed~System.Byte[]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'byte.Parse(string)' could vary based on the current user's locale settings. Replace this call in 'ConfidentialAssetsHelper.LoadArrayFromFile(string)' with a call to 'byte.Parse(string, IFormatProvider)'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.LoadArrayFromFile(System.String)~System.Byte[]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2208:Method MLSAG_Gen passes 'PK matrix is not rectangular' as the paramName argument to a ArgumentException constructor. Replace this argument with one of the method's parameter names. Note that the provided parameter name should have the exact casing as declared on the method.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.MLSAG_Gen(System.Byte[],System.Byte[][][],System.Byte[][],System.Int32,System.Int32)~O10.Crypto.MgSig")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'MgSig ConfidentialAssetsHelper.MLSAG_Gen(byte[] message, byte[][][] pk, byte[][] sk, int index, int dsRows)' passes a literal string as parameter 'message' of a call to 'ArgumentException.ArgumentException(string message, string paramName)'. Retrieve the following string(s) from a resource table instead: \"pk\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.MLSAG_Gen(System.Byte[],System.Byte[][][],System.Byte[][],System.Int32,System.Int32)~O10.Crypto.MgSig")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2208:Method Mlsag_Sign passes parameter name 'ss' as the message argument to a ArgumentException constructor. Replace this argument with a descriptive message and pass the parameter name in the correct position.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.Mlsag_Sign(System.Byte[],System.Byte[][],System.Byte[][],System.Int32,System.Int32,System.Byte[][])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'bool ConfidentialAssetsHelper.Mlsag_Sign(byte[] c, byte[][] xx, byte[][] alpha, int rows, int dsRows, byte[][] ss)' passes a literal string as parameter 'message' of a call to 'ArgumentException.ArgumentException(string message, string paramName)'. Retrieve the following string(s) from a resource table instead: \"xx\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.Mlsag_Sign(System.Byte[],System.Byte[][],System.Byte[][],System.Int32,System.Int32,System.Byte[][])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'bool ConfidentialAssetsHelper.MLSAG_Ver(byte[] message, byte[][][] pk, MgSig rv, int dsRows)' passes a literal string as parameter 'message' of a call to 'ArgumentException.ArgumentException(string message, string paramName)'. Retrieve the following string(s) from a resource table instead: \"dsRows\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.MLSAG_Ver(System.Byte[],System.Byte[][][],O10.Crypto.MgSig,System.Int32)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2208:Method MLSAG_Ver passes parameter name 'dsRows' as the message argument to a ArgumentException constructor. Replace this argument with a descriptive message and pass the parameter name in the correct position.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.MLSAG_Ver(System.Byte[],System.Byte[][][],O10.Crypto.MgSig,System.Int32)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'int.Parse(string)' could vary based on the current user's locale settings. Replace this call in 'ConfidentialAssetsHelper.ParseString(string)' with a call to 'int.Parse(string, IFormatProvider)'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.ParseString(System.String)~System.Int32[]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2208:Method ProveRctMG passes parameter name 'inSk' as the message argument to a ArgumentException constructor. Replace this argument with a descriptive message and pass the parameter name in the correct position.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.ProveRctMG(System.Byte[],O10.Crypto.CtTuple[][],O10.Crypto.CtTuple[],O10.Crypto.CtTuple[],O10.Crypto.CtTuple[],System.Int32)~O10.Crypto.MgSig")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'MgSig ConfidentialAssetsHelper.ProveRctMG(byte[] message, CtTuple[][] pubs, CtTuple[] inSk, CtTuple[] outSk, CtTuple[] outPk, int index)' passes a literal string as parameter 'message' of a call to 'ArgumentException.ArgumentException(string message, string paramName)'. Retrieve the following string(s) from a resource table instead: \"inSk\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.ProveRctMG(System.Byte[],O10.Crypto.CtTuple[][],O10.Crypto.CtTuple[],O10.Crypto.CtTuple[],O10.Crypto.CtTuple[],System.Int32)~O10.Crypto.MgSig")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'MgSig ConfidentialAssetsHelper.ProveRctMG(byte[] message, CtTuple[][] pubs, CtTuple[] inSk, CtTuple[] outSk, CtTuple[] outPk, int index)', validate parameter 'inSk' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.ProveRctMG(System.Byte[],O10.Crypto.CtTuple[][],O10.Crypto.CtTuple[],O10.Crypto.CtTuple[],O10.Crypto.CtTuple[],System.Int32)~O10.Crypto.MgSig")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'byte[] ConfidentialAssetsHelper.SumScalars(params byte[][] scalars)' passes a literal string as parameter 'message' of a call to 'IndexOutOfRangeException.IndexOutOfRangeException(string message)'. Retrieve the following string(s) from a resource table instead: \"All scalars must of 32 bytes length\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.SumScalars(System.Byte[][])~System.Byte[]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'bool ConfidentialAssetsHelper.VerifyBorromeanRingSignature(BorromeanRingSignatureEx borromeanRingSignature, byte[] msg, GroupElementP3[][] pubkeys)' passes a literal string as parameter 'message' of a call to 'ArgumentOutOfRangeException.ArgumentOutOfRangeException(string paramName, string message)'. Retrieve the following string(s) from a resource table instead: \"number of rings cannot be less than 1\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerifyBorromeanRingSignature(O10.Core.Cryptography.BorromeanRingSignatureEx,System.Byte[],Chaos.NaCl.Internal.Ed25519Ref10.GroupElementP3[][])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'bool ConfidentialAssetsHelper.VerifyBorromeanRingSignature(BorromeanRingSignatureEx borromeanRingSignature, byte[] msg, byte[][][] pubkeys)', validate parameter 'borromeanRingSignature' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerifyBorromeanRingSignature(O10.Core.Cryptography.BorromeanRingSignatureEx,System.Byte[],System.Byte[][][])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'bool ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(SurjectionProof surjectionProof, byte[] assetCommitment, byte[][] assetIds)' passes a literal string as parameter 'message' of a call to 'ArgumentOutOfRangeException.ArgumentOutOfRangeException(string paramName, string message)'. Retrieve the following string(s) from a resource table instead: \"number of issuance keys does not match length of assetID list\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerifyIssuanceSurjectionProof(O10.Core.Cryptography.SurjectionProof,System.Byte[],System.Byte[][])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'bool ConfidentialAssetsHelper.VerifyIssuanceSurjectionProof(SurjectionProof surjectionProof, byte[] assetCommitment, byte[][] assetIds)', validate parameter 'surjectionProof' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerifyIssuanceSurjectionProof(O10.Core.Cryptography.SurjectionProof,System.Byte[],System.Byte[][])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'bool ConfidentialAssetsHelper.VerifyRingSignature(byte[] msg, byte[] keyImage, byte[][] pubs, RingSignature[] signatures)', validate parameter 'signatures' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerifyRingSignature(System.Byte[],System.Byte[],System.Byte[][],O10.Core.Cryptography.RingSignature[])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'bool ConfidentialAssetsHelper.VerifySurjectionProof(SurjectionProof assetRangeProof, byte[] assetCommitment, params byte[][] aux)', validate parameter 'assetRangeProof' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerifySurjectionProof(O10.Core.Cryptography.SurjectionProof,System.Byte[],System.Byte[][])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'bool ConfidentialAssetsHelper.VerifyValueRangeProof(RangeProof valueRangeProof, byte[] assetCommitmentBytes, byte[] valueCommitmentBytes)', validate parameter 'valueRangeProof' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerifyValueRangeProof(O10.Core.Cryptography.RangeProof,System.Byte[],System.Byte[])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'bool ConfidentialAssetsHelper.VerRange(byte[] C, RangeProof rangeProof, byte[] assetCommitment)', validate parameter 'rangeProof' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerRange(System.Byte[],O10.Core.Cryptography.RangeProof,System.Byte[])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'bool ConfidentialAssetsHelper.VerRange(byte[] C, RangeProof rangeProof, byte[] assetCommitment)' passes a literal string as parameter 'message' of a call to 'Exception.Exception(string message)'. Retrieve the following string(s) from a resource table instead: \"point conv failed\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerRange(System.Byte[],O10.Core.Cryptography.RangeProof,System.Byte[])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'VerRange' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerRange(System.Byte[],O10.Core.Cryptography.RangeProof,System.Byte[])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'bool ConfidentialAssetsHelper.VerRctMG(MgSig mg, CtTuple[][] pubs, CtTuple[] outPk, byte[] message)', validate parameter 'mg' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerRctMG(O10.Crypto.MgSig,O10.Crypto.CtTuple[][],O10.Crypto.CtTuple[],System.Byte[])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2208:Method VerRctMG passes parameter name 'pubs' as the message argument to a ArgumentException constructor. Replace this argument with a descriptive message and pass the parameter name in the correct position.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerRctMG(O10.Crypto.MgSig,O10.Crypto.CtTuple[][],O10.Crypto.CtTuple[],System.Byte[])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'bool ConfidentialAssetsHelper.VerRctMG(MgSig mg, CtTuple[][] pubs, CtTuple[] outPk, byte[] message)' passes a literal string as parameter 'message' of a call to 'ArgumentException.ArgumentException(string message, string paramName)'. Retrieve the following string(s) from a resource table instead: \"pubs\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.ConfidentialAssets.CryptoHelper.VerRctMG(O10.Crypto.MgSig,O10.Crypto.CtTuple[][],O10.Crypto.CtTuple[],System.Byte[])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object, object)' could vary based on the current user's locale settings. Replace this call in 'WrongSecretKeysNumberException.WrongSecretKeysNumberException(string, int)' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.Exceptions.WrongSecretKeysNumberException.#ctor(System.String,System.Int32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object, object)' could vary based on the current user's locale settings. Replace this call in 'WrongSecretKeysNumberException.WrongSecretKeysNumberException(string, int, Exception)' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.Exceptions.WrongSecretKeysNumberException.#ctor(System.String,System.Int32,System.Exception)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'UtxoSigningService.UtxoSigningService(IIdentityKeyProvidersRegistry identityKeyProvidersRegistry, ILoggerService loggerService)', validate parameter 'loggerService' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.StealthSigningService.#ctor(O10.Core.Identity.IIdentityKeyProvidersRegistry,O10.Core.Logging.ILoggerService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void UtxoSigningService.Sign(IPacket packet, object args = null)', validate parameter 'packet' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.StealthSigningService.Sign(O10.Core.Models.IPacket,System.Object)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object, object)' could vary based on the current user's locale settings. Replace this call in 'UtxoSigningService.Sign(IPacket, [object])' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.StealthSigningService.Sign(O10.Core.Models.IPacket,System.Object)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object, object)' could vary based on the current user's locale settings. Replace this call in 'UtxoSigningService.Verify(IPacket)' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Crypto.StealthSigningService.Verify(O10.Core.Models.IPacket)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Crypto.Ed25519SigningService.PublicKeys")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Crypto.CtTuple.Dest")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Crypto.CtTuple.Mask")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Crypto.MgSig.CC")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Crypto.MgSig.SS")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Crypto.StealthSigningService.PublicKeys")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1032:Add the following constructor to WrongSecretKeysNumberException: public WrongSecretKeysNumberException(string message).", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Crypto.Exceptions.WrongSecretKeysNumberException")]