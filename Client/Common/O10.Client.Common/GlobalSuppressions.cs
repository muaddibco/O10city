﻿
// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.PacketsExtractorBase._clientCryptoService")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.PacketsExtractorBase._logger")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.TransactionsServiceBase._hashCalculation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.TransactionsServiceBase._identityKeyProvider")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.TransactionsServiceBase._logger")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.TransactionsServiceBase._observers")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.TransactionsServiceBase._proofOfWorkCalculation")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.TransactionsServiceBase._signingService")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.WalletSynchronizer._accountId")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.WalletSynchronizer._clientCryptoService")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.WalletSynchronizer._dataAccessService")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Communication.WalletSynchronizer._logger")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1707:Remove the underscores from member name O10.Client.Common.Configuration.RestApiConfiguration.SECTION_NAME.", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Configuration.RestApiConfiguration.SECTION_NAME")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Services.WitnessPackageProviderBase._accountId")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Services.WitnessPackageProviderBase._cancellationToken")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Services.WitnessPackageProviderBase._dataAccessService")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Services.WitnessPackageProviderBase._gatewayService")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Services.WitnessPackageProviderBase._lastObtainedCombinedBlockHeight")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.Common.Services.WitnessPackageProviderBase._logger")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'GatewayService.GatewayService(IRestClientService restClientService, ILoggerService loggerService)', validate parameter 'loggerService' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.GatewayService.#ctor(O10.Client.Common.Interfaces.IRestClientService,O10.Core.Logging.ILoggerService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1055:Change the return type of method GatewayService.GetNotificationsHubUri() from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.GatewayService.GetNotificationsHubUri~System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1054:Change the type of parameter gatewayUri of method GatewayService.Initialize(string, CancellationToken) from string to System.Uri, or provide an overload to GatewayService.Initialize(string, CancellationToken) that allows gatewayUri to be passed as a System.Uri object.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.GatewayService.Initialize(System.String,System.Threading.CancellationToken)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'Initialize' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.GatewayService.Initialize(System.String,System.Threading.CancellationToken)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'IFlurlRequest RestClientService.Request(Url url)', validate parameter 'url' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.RestClientService.Request(Flurl.Url)~Flurl.Http.IFlurlRequest")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2000:Call System.IDisposable.Dispose on object created by 'new FlurlClient(httpClient)' before all references to it are out of scope.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.RestClientService.Request(System.String)~Flurl.Http.IFlurlRequest")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1054:Change the type of parameter uri of method RestClientService.Request(string) from string to System.Uri, or provide an overload to RestClientService.Request(string) that allows uri to be passed as a System.Uri object.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.RestClientService.Request(System.String)~Flurl.Http.IFlurlRequest")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'InitializeInner' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.SignalRWitnessPackagesProvider.InitializeInner")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'bool StatePacketsExtractor.CheckPacketWitness(PacketWitness packetWitness)', validate parameter 'packetWitness' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.StatePacketsExtractor.CheckPacketWitness(O10.Core.Models.PacketWitness)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'bool UtxoPacketsExtractor.CheckPacketWitness(PacketWitness packetWitness)', validate parameter 'packetWitness' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.StealthPacketsExtractor.CheckPacketWitness(O10.Core.Models.PacketWitness)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1816:Change WalletSynchronizer.Dispose() to call GC.SuppressFinalize(object). This will prevent derived types that introduce a finalizer from needing to re-implement 'IDisposable' to call it.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Communication.WalletSynchronizer.Dispose")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void StateClientCryptoService.DecodeEcdhTuple(EcdhTupleIP ecdhTupleCA, byte[] transactionKey, out byte[] issuer, out byte[] payload)', validate parameter 'ecdhTupleCA' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Crypto.StateClientCryptoService.DecodeEcdhTuple(O10.Core.Cryptography.EcdhTupleIP,System.Byte[],System.Byte[]@,System.Byte[]@)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void StateClientCryptoService.DecodeEcdhTuple(EcdhTupleProofs ecdhTuple, byte[] transactionKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload)', validate parameter 'ecdhTuple' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Crypto.StateClientCryptoService.DecodeEcdhTuple(O10.Core.Cryptography.EcdhTupleProofs,System.Byte[],System.Byte[]@,System.Byte[]@,System.Byte[]@,System.Byte[]@)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void UtxoClientCryptoService.DecodeEcdhTuple(EcdhTupleIP ecdhTuple, byte[] transactionKey, out byte[] issuer, out byte[] payload)', validate parameter 'ecdhTuple' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Crypto.StealthClientCryptoService.DecodeEcdhTuple(O10.Core.Cryptography.EcdhTupleIP,System.Byte[],System.Byte[]@,System.Byte[]@)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void UtxoClientCryptoService.DecodeEcdhTuple(EcdhTupleProofs ecdhTuple, byte[] transactionKey, out byte[] blindingFactor, out byte[] assetId, out byte[] issuer, out byte[] payload)', validate parameter 'ecdhTuple' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Crypto.StealthClientCryptoService.DecodeEcdhTuple(O10.Core.Cryptography.EcdhTupleProofs,System.Byte[],System.Byte[]@,System.Byte[]@,System.Byte[]@,System.Byte[]@)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object)' could vary based on the current user's locale settings. Replace this call in 'AccountNotFoundException.AccountNotFoundException(long)' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Exceptions.AccountNotFoundException.#ctor(System.Int64)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object)' could vary based on the current user's locale settings. Replace this call in 'AccountNotFoundException.AccountNotFoundException(long, Exception)' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Exceptions.AccountNotFoundException.#ctor(System.Int64,System.Exception)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'Tuple<byte[], byte[]> AccountExtensions.DecryptUtxoKeys(Account account, string passphrase, byte[] aesInitializationVector)', validate parameter 'account' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Extensions.AccountExtensions.DecryptStealthKeys(O10.Client.DataLayer.Model.Account,System.String,System.Byte[])~System.Tuple{System.Byte[],System.Byte[]}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2000:Call System.IDisposable.Dispose on object created by 'SHA256.Create()' before all references to it are out of scope.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Extensions.AccountExtensions.GetSecretKeys(O10.Client.DataLayer.Model.Account,System.String,System.Byte[])~System.Tuple{System.Byte[],System.Byte[]}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'Tuple<byte[], byte[]> AccountExtensions.GetSecretKeys(Account account, string passphrase, byte[] aesInitializationVector)', validate parameter 'account' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Extensions.AccountExtensions.GetSecretKeys(O10.Client.DataLayer.Model.Account,System.String,System.Byte[])~System.Tuple{System.Byte[],System.Byte[]}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'AssetsService.AssetsService(IHashCalculationsRepository hashCalculationsRepository, ISchemeResolverService schemeResolverService)', validate parameter 'hashCalculationsRepository' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.AssetsService.#ctor(O10.Core.HashCalculations.IHashCalculationsRepository,O10.Client.Common.Interfaces.ISchemeResolverService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'FacesService.FacesService(ILoggerService loggerService, IDataAccessService externalDataAccessService)', validate parameter 'loggerService' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.FacesService.#ctor(O10.Core.Logging.ILoggerService,O10.Client.DataLayer.Services.IDataAccessService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'Task<Guid> FacesService.AddPerson(PersonFaceData facesData)', validate parameter 'facesData' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.FacesService.AddPerson(O10.Client.Common.Identities.PersonFaceData)~System.Threading.Tasks.Task{System.Guid}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'DetectedFaces' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.FacesService.DetectedFaces(System.String)~System.Collections.Generic.IList{Microsoft.Azure.CognitiveServices.Vision.Face.Models.DetectedFace}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1816:Change FacesService.Dispose() to call GC.SuppressFinalize(object). This will prevent derived types that introduce a finalizer from needing to re-implement 'IDisposable' to call it.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.FacesService.Dispose")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'Initialize' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.FacesService.Initialize~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'Task<Guid> FacesService.ReplacePersonFace(PersonFaceData facesData)', validate parameter 'facesData' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.FacesService.ReplacePersonFace(O10.Client.Common.Identities.PersonFaceData)~System.Threading.Tasks.Task{System.Guid}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'VerifyPerson' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.FacesService.VerifyPerson(System.String,System.Guid,System.String)~System.Threading.Tasks.Task{System.Tuple{System.Boolean,System.Double}}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'IdentityAttributesService.IdentityAttributesService(IHashCalculationsRepository hashCalculationsRepository, ISchemeResolverService schemeResolverService)', validate parameter 'hashCalculationsRepository' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.IdentityAttributesService.#ctor(O10.Core.HashCalculations.IHashCalculationsRepository,O10.Client.Common.Interfaces.ISchemeResolverService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1307:The behavior of 'string.Equals(string)' could vary based on the current user's locale settings. Replace this call in 'O10.Client.Common.Identities.IdentityAttributesService.GetValidationType(string)' with a call to 'string.Equals(string, System.StringComparison)'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Identities.IdentityAttributesService.GetValidationType(System.String)~O10.Client.DataLayer.Enums.ValidationType")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1055:Change the return type of method IGatewayService.GetNotificationsHubUri() from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Interfaces.IGatewayService.GetNotificationsHubUri~System.String")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1054:Change the type of parameter gatewayUri of method IGatewayService.Initialize(string, CancellationToken) from string to System.Uri, or provide an overload to IGatewayService.Initialize(string, CancellationToken) that allows gatewayUri to be passed as a System.Uri object.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Interfaces.IGatewayService.Initialize(System.String,System.Threading.CancellationToken)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1054:Change the type of parameter uri of method IRestClientService.Request(string) from string to System.Uri, or provide an overload to IRestClientService.Request(string) that allows uri to be passed as a System.Uri object.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Interfaces.IRestClientService.Request(System.String)~Flurl.Http.IFlurlRequest")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2000:Call System.IDisposable.Dispose on object created by 'SHA256.Create()' before all references to it are out of scope.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Services.AccountsService.DecryptStateKeys(O10.Client.DataLayer.Model.Account,System.String)~System.Byte[]")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2000:Call System.IDisposable.Dispose on object created by 'SHA256.Create()' before all references to it are out of scope.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Services.AccountsService.EncryptKeys(O10.Client.DataLayer.Enums.AccountType,System.String,System.Byte[],System.Byte[],System.Byte[]@,System.Byte[]@)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2000:Call System.IDisposable.Dispose on object created by 'SHA256.Create()' before all references to it are out of scope.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Services.AccountsService.GetSecretKeys(O10.Client.DataLayer.Model.Account,System.String)~System.Tuple{System.Byte[],System.Byte[]}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1303:Method 'void AccountsService.ResetAccount(long accountId, string passphrase)' passes a literal string as parameter 'message' of a call to 'IndexOutOfRangeException.IndexOutOfRangeException(string message)'. Retrieve the following string(s) from a resource table instead: \"accountId\".", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Services.AccountsService.ResetAccount(System.Int64,System.String)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'InherenceVerifiersService.InherenceVerifiersService(IRestClientService restClientService, IConfigurationService configurationService)', validate parameter 'configurationService' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Services.InherenceVerifiersService.#ctor(O10.Client.Common.Interfaces.IRestClientService,O10.Core.Configuration.IConfigurationService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'RelationsProofsValidationService.RelationsProofsValidationService(IGatewayService gatewayService, IAssetsService assetsService, IIdentityAttributesService identityAttributesService, IConfigurationService configurationService, IDataAccessService dataAccessService, ILoggerService loggerService)', validate parameter 'configurationService' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Services.RelationsProofsValidationService.#ctor(O10.Client.Common.Interfaces.IGatewayService,O10.Client.Common.Interfaces.IAssetsService,O10.Client.Common.Interfaces.IIdentityAttributesService,O10.Core.Configuration.IConfigurationService,O10.Client.DataLayer.Services.IDataAccessService,O10.Core.Logging.ILoggerService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'SchemeResolverService.SchemeResolverService(IRestClientService restClientService, IConfigurationService configurationService, ILoggerService loggerService)', validate parameter 'loggerService' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Services.SchemeResolverService.#ctor(O10.Client.Common.Interfaces.IRestClientService,O10.Core.Configuration.IConfigurationService,O10.Core.Logging.ILoggerService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'StoreRegistrationCommitment' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Services.SchemeResolverService.StoreRegistrationCommitment(System.String,System.String,System.String,System.String)~System.Threading.Tasks.Task{System.Boolean}")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'WitnessPackageProviderBase.WitnessPackageProviderBase(IGatewayService gatewayService, IDataAccessService dataAccessService, ILoggerService loggerService)', validate parameter 'loggerService' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.Common.Services.WitnessPackageProviderBase.#ctor(O10.Client.Common.Interfaces.IGatewayService,O10.Client.DataLayer.Services.IDataAccessService,O10.Core.Logging.ILoggerService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property IRestApiConfiguration.ConsentManagementUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.IRestApiConfiguration.ConsentManagementUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property IRestApiConfiguration.GatewayUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.IRestApiConfiguration.GatewayUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property IRestApiConfiguration.InherenceUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.IRestApiConfiguration.InherenceUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property IRestApiConfiguration.SamlIdpUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.IRestApiConfiguration.SamlIdpUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property IRestApiConfiguration.SchemaResolutionUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.IRestApiConfiguration.SchemaResolutionUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property RestApiConfiguration.ConsentManagementUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.RestApiConfiguration.ConsentManagementUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property RestApiConfiguration.GatewayUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.RestApiConfiguration.GatewayUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property RestApiConfiguration.InherenceUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.RestApiConfiguration.InherenceUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property RestApiConfiguration.SamlIdpUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.RestApiConfiguration.SamlIdpUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property RestApiConfiguration.SchemaResolutionUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Configuration.RestApiConfiguration.SchemaResolutionUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property IssuerActionDetails.ActionUri from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Entities.IssuerActionDetails.ActionUri")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2227:Change 'ValidationCriterionTypes' to be read-only by removing the property setter.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.Common.Identities.IdentityAttributeValidationDescriptor.ValidationCriterionTypes")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1001:Type 'SignalRWitnessPackagesProvider' owns disposable field(s) '_witnessPackages' but is not disposable", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Client.Common.Communication.SignalRWitnessPackagesProvider")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1032:Add the following constructor to AccountNotFoundException: public AccountNotFoundException(string message).", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Client.Common.Exceptions.AccountNotFoundException")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1032:Add the following constructor to BindingKeyNotInitializedException: public BindingKeyNotInitializedException(string message).", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Client.Common.Exceptions.BindingKeyNotInitializedException")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1032:Add the following constructor to SchemeResolverServiceNotInitializedException: public SchemeResolverServiceNotInitializedException(string message, Exception innerException).", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Client.Common.Exceptions.SchemeResolverServiceNotInitializedException")]
