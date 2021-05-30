﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1707:Remove the underscores from member ", Justification = "<Pending>", Scope = "module")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.DataLayer.AttributesScheme.AttributesSchemes.AttributeSchemes")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1707:Remove the underscores from member name O10.Client.DataLayer.Configuration.ClientDataContextConfiguration.SECTION_NAME.", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.DataLayer.Configuration.ClientDataContextConfiguration.SECTION_NAME")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member", Target = "~F:O10.Client.DataLayer.DataContext._connectionString")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void DataContext.OnModelCreating(ModelBuilder modelBuilder)', validate parameter 'modelBuilder' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.DataContext.OnModelCreating(Microsoft.EntityFrameworkCore.ModelBuilder)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object)' could vary based on the current user's locale settings. Replace this call in 'AccountDoesNotExistException.AccountDoesNotExistException(long)' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Exceptions.AccountDoesNotExistException.#ctor(System.Int64)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1305:The behavior of 'string.Format(string, object)' could vary based on the current user's locale settings. Replace this call in 'AccountDoesNotExistException.AccountDoesNotExistException(long, Exception)' with a call to 'string.Format(IFormatProvider, string, params object[])'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Exceptions.AccountDoesNotExistException.#ctor(System.Int64,System.Exception)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'DataAccessService.DataAccessService(IEnumerable<IDataContext> dataContexts, IConfigurationService configurationService, ILoggerService loggerService)', validate parameter 'configurationService' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.#ctor(System.Collections.Generic.IEnumerable{O10.Core.IDataContext},O10.Core.Configuration.IConfigurationService,O10.Core.Logging.ILoggerService)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'long DataAccessService.AddUserRootAttribute(long accountId, UserRootAttribute attribute)', validate parameter 'attribute' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.AddUserRootAttribute(System.Int64,O10.Client.DataLayer.Model.UserRootAttribute)~System.Int64")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'Identity DataAccessService.CreateIdentity(long accountId, string description, (string attrName, string content)[] attributes)', validate parameter 'attributes' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.CreateIdentity(System.Int64,System.String,System.[])~O10.Client.DataLayer.Model.Identity")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'DbInitialize' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.DbInitialize")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1307:The behavior of 'string.Equals(string)' could vary based on the current user's locale settings. Replace this call in 'O10.Client.DataLayer.Services.DataAccessService.DbInitialize()' with a call to 'string.Equals(string, System.StringComparison)'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.DbInitialize")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1820:Test for empty strings using 'string.Length' property or 'string.IsNullOrEmpty' method instead of an Equality check.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.DeleteNonConfirmedUserRootAttribute(System.Int64,System.String)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1820:Test for empty strings using 'string.Length' property or 'string.IsNullOrEmpty' method instead of an Equality check.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.DuplicateUserAccount(System.Int64,System.String)~System.Int64")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1307:The behavior of 'string.Equals(string)' could vary based on the current user's locale settings. Replace this call in 'O10.Client.DataLayer.Services.DataAccessService.GetIdentityByAttribute(long, string, string)' with a call to 'string.Equals(string, System.StringComparison)'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.GetIdentityByAttribute(System.Int64,System.String,System.String)~O10.Client.DataLayer.Model.Identity")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1031:Modify 'Initialize' to catch a more specific allowed exception type, or rethrow the exception.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.Initialize~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void DataAccessService.SetConsentManagementSettings(ConsentManagementSettings consentManagementSettings)', validate parameter 'consentManagementSettings' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.SetConsentManagementSettings(O10.Client.DataLayer.Model.ConsentManagement.ConsentManagementSettings)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void DataAccessService.SetUserSettings(long accountId, UserSettings userSettings)', validate parameter 'userSettings' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.SetUserSettings(System.Int64,O10.Client.DataLayer.Model.UserSettings)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1054:Change the type of parameter singleLogoutUrl of method DataAccessService.StoreSamlServiceProvider(string, string) from string to System.Uri, or provide an overload to DataAccessService.StoreSamlServiceProvider(string, string) that allows singleLogoutUrl to be passed as a System.Uri object.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.StoreSamlServiceProvider(System.String,System.String)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1062:In externally visible method 'void DataAccessService.UpdateConfirmedRootAttribute(UserRootAttribute userRootAttribute)', validate parameter 'userRootAttribute' is non-null before using it. If appropriate, throw an ArgumentNullException when the argument is null or add a Code Contract precondition asserting non-null argument.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.UpdateConfirmedRootAttribute(O10.Client.DataLayer.Model.UserRootAttribute)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1307:The behavior of 'string.Equals(string)' could vary based on the current user's locale settings. Replace this call in 'O10.Client.DataLayer.Services.DataAccessService.UpdateSpDocumentChangeRecord(long, string, ulong)' with a call to 'string.Equals(string, System.StringComparison)'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.UpdateSpDocumentChangeRecord(System.Int64,System.String,System.UInt64)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1307:The behavior of 'string.Equals(string)' could vary based on the current user's locale settings. Replace this call in 'O10.Client.DataLayer.Services.DataAccessService.UpdateSpDocumentSignature(long, string, ulong, ulong, byte[])' with a call to 'string.Equals(string, System.StringComparison)'.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.DataAccessService.UpdateSpDocumentSignature(System.Int64,System.String,System.UInt64,System.UInt64,System.Byte[])~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1716:In virtual/interface member IDataAccessService.AddAttributeToScheme(string, string, string, string, string), rename parameter alias so that it no longer conflicts with the reserved language keyword 'Alias'. Using a reserved keyword as the name of a parameter on a virtual/interface member makes it harder for consumers in other languages to override/implement the member.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.IDataAccessService.AddAttributeToScheme(System.String,System.String,System.String,System.String,System.String)~System.Int64")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1054:Change the type of parameter singleLogoutUrl of method IDataAccessService.StoreSamlServiceProvider(string, string) from string to System.Uri, or provide an overload to IDataAccessService.StoreSamlServiceProvider(string, string) that allows singleLogoutUrl to be passed as a System.Uri object.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.IDataAccessService.StoreSamlServiceProvider(System.String,System.String)~System.Boolean")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1716:In virtual/interface member IDataAccessService.UpdateScenarioSessionStep(long, int), rename parameter step so that it no longer conflicts with the reserved language keyword 'Step'. Using a reserved keyword as the name of a parameter on a virtual/interface member makes it harder for consumers in other languages to override/implement the member.", Justification = "<Pending>", Scope = "member", Target = "~M:O10.Client.DataLayer.Services.IDataAccessService.UpdateScenarioSessionStep(System.Int64,System.Int32)")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Entities.AllowedSignerEntity.BlindingFactor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Entities.AllowedSignerEntity.GroupCommitment")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2227:Change 'AllowedSigners' to be read-only by removing the property setter.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Entities.SignedDocumentEntity.AllowedSigners")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.Account.PublicSpendKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.Account.PublicViewKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.Account.SecretSpendKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.Account.SecretViewKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.AutoLogin.SecretKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2227:Change 'Attributes' to be read-only by removing the property setter.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.Identity.Attributes")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.IdentityAttribute.Commitment")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1056:Change the type of property SamlServiceProvider.SingleLogoutUrl from string to System.Uri.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SamlServiceProvider.SingleLogoutUrl")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.ServiceProviderRegistration.Commitment")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SpAttribute.AssetId")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SpAttribute.IssuingCommitment")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SpAttribute.OriginalBlindingFactor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SpAttribute.OriginalCommitment")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2227:Change 'AllowedSigners' to be read-only by removing the property setter.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SpDocument.AllowedSigners")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA2227:Change 'DocumentSignatures' to be read-only by removing the property setter.", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SpDocument.DocumentSignatures")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SpDocumentSignature.DocumentSignRecord")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SpIdenitityValidation.GroupIdCriterion")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SystemSettings.BiometricSecretKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.SystemSettings.InitializationVector")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.UserRootAttribute.AssetId")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.UserRootAttribute.AnchoringOriginationCommitment")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.UserRootAttribute.LastBlindingFactor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.UserRootAttribute.LastCommitment")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.UserRootAttribute.LastDestinationKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.UserRootAttribute.LastTransactionKey")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.UserRootAttribute.OriginalBlindingFactor")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1819:Properties should not return arrays", Justification = "<Pending>", Scope = "member", Target = "~P:O10.Client.DataLayer.Model.UserRootAttribute.IssuanceCommitment")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1028:If possible, make the underlying type of AccountType System.Int32 instead of byte.", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Client.DataLayer.Enums.AccountType")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1028:If possible, make the underlying type of ServiceProviderType System.Int32 instead of short.", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Client.DataLayer.Enums.ServiceProviderType")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1028:If possible, make the underlying type of ValidationType System.Int32 instead of ushort.", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Client.DataLayer.Enums.ValidationType")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1032:Add the following constructor to AccountDoesNotExistException: public AccountDoesNotExistException(string message).", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Client.DataLayer.Exceptions.AccountDoesNotExistException")]
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Build", "CA1724:The type name Identity conflicts in whole or in part with the namespace name 'O10.Core.Identity'. Change either name to eliminate the conflict.", Justification = "<Pending>", Scope = "type", Target = "~T:O10.Client.DataLayer.Model.Identity")]