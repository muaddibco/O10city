using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using O10.Core.ExtensionMethods;
using O10.Client.Common.Entities;
using O10.Client.DataLayer.Enums;
using O10.Client.DataLayer.Services;
using O10.Crypto.ConfidentialAssets;
using Chaos.NaCl;
using O10.Client.Common.Interfaces;
using O10.Core.Architecture;
using O10.Client.Common.Exceptions;
using O10.Core;
using O10.Core.HashCalculations;
using System.Linq;
using O10.Client.DataLayer.Model;
using O10.Core.Translators;

namespace O10.Client.Common.Services
{
    [RegisterDefaultImplementation(typeof(IAccountsService), Lifetime = LifetimeManagement.Singleton)]
    public class AccountsService : IAccountsService
    {
        private readonly IDataAccessService _dataAccessService;
        private readonly IGatewayService _gatewayService;
        private readonly ITranslatorsRepository _translatorsRepository;
        private readonly IHashCalculation _hashCalculation;

        public AccountsService(IDataAccessService dataAccessService,
                               IGatewayService gatewayService,
                               IHashCalculationsRepository hashCalculationsRepository,
                               ITranslatorsRepository translatorsRepository)
        {
            _dataAccessService = dataAccessService;
            _gatewayService = gatewayService;
            _translatorsRepository = translatorsRepository;
            _hashCalculation = hashCalculationsRepository.Create(Globals.DEFAULT_HASH);
        }

        public AccountDescriptor Authenticate(long accountId, string password)
        {
            Account account = _dataAccessService.GetAccount(accountId);

            if (account == null)
            {
                throw new AccountNotFoundException(accountId);
            }

            var res = account.AccountType == AccountType.User
                ? AuthenticateStealthAccount(new AuthenticationInput { Password = password, Account = account })
                : AuthenticateStateAccount(new AuthenticationInput { Password = password, Account = account });

            return res;
        }

        private AccountDescriptor AuthenticateStateAccount(AuthenticationInput authenticationInput)
        {
            AccountDescriptor accountDescriptor = null;
            bool res = IsPasswordValid(authenticationInput.Account, authenticationInput.Password);
            if (res)
            {
                byte[] pwdHash = _hashCalculation.CalculateHash(Encoding.UTF8.GetBytes(authenticationInput.Password));

                accountDescriptor = TranslateToAccountDescriptor(authenticationInput.Account, pwdHash);

                if (!authenticationInput.Account.IsPrivate && !_dataAccessService.IsAutoLoginExist(authenticationInput.Account.AccountId))
                {
                    _dataAccessService.AddAutoLogin(authenticationInput.Account.AccountId, accountDescriptor.SecretSpendKey);
                }
            }

            return accountDescriptor;
        }

        private AccountDescriptor? AuthenticateStealthAccount(AuthenticationInput authenticationInput)
        {
            AccountDescriptor? accountDescriptor = null;

            bool res = IsPasswordValid(authenticationInput.Account, authenticationInput.Password);
            if (res)
            {
                byte[] pwdHash = _hashCalculation.CalculateHash(Encoding.UTF8.GetBytes(authenticationInput.Password));

                accountDescriptor = TranslateToAccountDescriptor(authenticationInput.Account, pwdHash);
            }

            return accountDescriptor;
        }

        protected AccountDescriptor TranslateToAccountDescriptor(Account? account, byte[]? pwdHash = null)
        {
            var accountDescriptor = _translatorsRepository.GetInstance<Account, AccountDescriptor>()?.Translate(account);
            if (accountDescriptor != null && pwdHash != null)
            {
                accountDescriptor.SecretSpendKey = CryptoHelper.SumScalars(account.SecretSpendKey, pwdHash);
                accountDescriptor.SecretViewKey = (account.AccountType == AccountType.User) ? CryptoHelper.SumScalars(account.SecretViewKey, pwdHash) : null;
                accountDescriptor.PwdHash = pwdHash;
            }

            return accountDescriptor;
        }

        public AccountDescriptor? FindByAlias(string alias)
        {
            var account = _dataAccessService.FindAccountByAlias(alias);

            var accountDescriptor = _translatorsRepository.GetInstance<Account, AccountDescriptor>()?.Translate(account);

            return accountDescriptor;
        }

        public long Create(AccountTypeDTO accountType, string? accountInfo = null, string? password = null, bool isPrivate = false) =>
            string.IsNullOrEmpty(password)
                ? AddNonEncryptedAccount(accountType)
                : AddEncryptedAccount(accountType, accountInfo, password, isPrivate);

        public void Update(long accountId, string accountInfo = null, string password = null) =>
            UpdateExistingAccount(accountId, accountInfo, password);

        public void ResetAccount(long accountId, string passphrase)
        {
            Account account = _dataAccessService.GetAccount(accountId);
            if (account == null)
            {
                throw new IndexOutOfRangeException(nameof(accountId));
            }

            byte[] secretSpendKey = CryptoHelper.GetRandomSeed();
            byte[] secretViewKey = (account.AccountType == AccountType.User) ? CryptoHelper.GetRandomSeed() : null;
            byte[] pwdHash = _hashCalculation.CalculateHash(Encoding.UTF8.GetBytes(passphrase));
            byte[] secretSpendKeyPwd = CryptoHelper.SumScalars(secretSpendKey, pwdHash);
            byte[] secretViewKeyPwd = (account.AccountType == AccountType.User) ? CryptoHelper.SumScalars(secretViewKey, pwdHash) : null;

            byte[] publicSpendKey = (account.AccountType == AccountType.User) ? CryptoHelper.GetPublicKey(secretSpendKeyPwd) : CryptoHelper.GetPublicKey(Ed25519.SecretKeyFromSeed(secretSpendKeyPwd));
            byte[] publicViewKey = (account.AccountType == AccountType.User) ? CryptoHelper.GetPublicKey(secretViewKeyPwd) : null;

            //EncryptKeys(account.AccountType, password, secretSpendKey, secretViewKey, out byte[] secretSpendKeyEnc, out byte[] secretViewKeyEnc);

            var combinedBlock = AsyncUtil.RunSync(() => _gatewayService.GetLastRegistryCombinedBlock());
            _dataAccessService.ResetAccount(accountId, secretSpendKey, secretViewKey, publicSpendKey, publicViewKey, combinedBlock.Height);
        }

        public void Override(AccountTypeDTO accountType, long accountId, byte[] secretSpendKey, byte[] secretViewKey, string password, long lastRegistryCombinedBlockHeight) =>
            OverrideEncryptedAccount(accountType, accountId, password, secretSpendKey, secretViewKey, lastRegistryCombinedBlockHeight);

        public void Delete(long accountId) =>
            _dataAccessService.RemoveAccount(accountId);

        public List<AccountDescriptor> GetAll() =>
            _dataAccessService.GetAccounts().Select(a => TranslateToAccountDescriptor(a)).ToList();

        public AccountDescriptor GetById(long accountId) =>
            TranslateToAccountDescriptor(_dataAccessService.GetAccount(accountId));

        public void Update(AccountDescriptor user, string password = null) => throw new NotImplementedException();

        #region Private Functions

        private long AddEncryptedAccount(AccountTypeDTO accountType, string accountInfo, string passphrase, bool isPrivate = false)
        {
            GenerateSecretKeys(accountType, out byte[] secretSpendKey, out byte[] secretViewKey);

            GeneratePasswordKeys(accountType, passphrase, secretSpendKey, secretViewKey, out byte[] publicSpendKey, out byte[] publicViewKey);

            //EncryptKeys(accountType, passphrase, secretSpendKey, secretViewKey, out byte[] secretSpendKeyEnc, out byte[] secretViewKeyEnc);

            var combinedBlock = AsyncUtil.RunSync(() => _gatewayService.GetLastRegistryCombinedBlock());
            long accountId = _dataAccessService.AddAccount((byte)accountType, accountInfo, secretSpendKey, secretViewKey, publicSpendKey, publicViewKey, combinedBlock?.Height ?? 0, isPrivate);

            if (accountType == AccountTypeDTO.User)
            {
                _dataAccessService.SetUserSettings(accountId, new UserSettings { IsAutoTheftProtection = true });
            }

            return accountId;
        }

        private long AddNonEncryptedAccount(AccountTypeDTO accountType)
        {
            GenerateSecretKeys(accountType, out byte[] secretSpendKey, out byte[] secretViewKey);
            var combinedBlock = AsyncUtil.RunSync(() => _gatewayService.GetLastRegistryCombinedBlock());
            long accountId = _dataAccessService.AddAccount((byte)accountType, null, secretSpendKey, secretViewKey, null, null, combinedBlock.Height, false);

            if (accountType == AccountTypeDTO.User)
            {
                _dataAccessService.SetUserSettings(accountId, new UserSettings { IsAutoTheftProtection = true });
            }

            return accountId;
        }

        private void UpdateExistingAccount(long accountId, string accountInfo, string passphrase)
        {
            Account account = _dataAccessService.GetAccount(accountId);
            GeneratePasswordKeys((AccountTypeDTO)account.AccountType, passphrase, account.SecretSpendKey, account.SecretViewKey, out byte[] publicSpendKey, out byte[] publicViewKey);
            _dataAccessService.UpdateAccount(accountId, accountInfo, publicSpendKey, publicViewKey);
        }

        private static void GenerateSecretKeys(AccountTypeDTO accountType, out byte[] secretSpendKey, out byte[] secretViewKey)
        {
            secretSpendKey = CryptoHelper.GetRandomSeed();
            secretViewKey = (accountType == AccountTypeDTO.User) ? CryptoHelper.GetRandomSeed() : null;
        }

        protected void GeneratePasswordKeys(AccountTypeDTO accountType, string passphrase, byte[] secretSpendKey, byte[] secretViewKey, out byte[] publicSpendKey, out byte[] publicViewKey)
        {
            byte[] pwdHash = _hashCalculation.CalculateHash(Encoding.UTF8.GetBytes(passphrase));
            byte[] secretSpendKeyPwd = CryptoHelper.SumScalars(secretSpendKey, pwdHash);
            byte[] secretViewKeyPwd = (accountType == AccountTypeDTO.User) ? CryptoHelper.SumScalars(secretViewKey, pwdHash) : null;

            publicSpendKey = (accountType == AccountTypeDTO.User) ? CryptoHelper.GetPublicKey(secretSpendKeyPwd) : CryptoHelper.GetPublicKey(Ed25519.SecretKeyFromSeed(secretSpendKeyPwd));
            publicViewKey = (accountType == AccountTypeDTO.User) ? CryptoHelper.GetPublicKey(secretViewKeyPwd) : null;
        }

        private void OverrideEncryptedAccount(AccountTypeDTO accountType, long accountId, string passphrase, byte[] secretSpendKey, byte[] secretViewKey, long lastRegistryCombinedBlockHeight)
        {
            GeneratePasswordKeys(accountType, passphrase, secretSpendKey, secretViewKey, out var publicSpendKey, out var publicViewKey);
            _dataAccessService.OverrideUserAccount(accountId, secretSpendKey, secretViewKey, publicSpendKey, publicViewKey, lastRegistryCombinedBlockHeight);
        }

        private void EncryptKeys(AccountType accountType, string passphrase, byte[] secretSpendKey, byte[] secretViewKey, out byte[] secretSpendKeyEnc, out byte[] secretViewKeyEnc)
        {
            secretSpendKeyEnc = null;
            secretViewKeyEnc = null;
            using (var aes = Aes.Create())
            {
                aes.IV = _dataAccessService.GetAesInitializationVector();
                byte[] passphraseBytes = Encoding.ASCII.GetBytes(passphrase);
                aes.Key = SHA256.Create().ComputeHash(passphraseBytes);
                aes.Padding = PaddingMode.None;

                secretSpendKeyEnc = aes.CreateEncryptor().TransformFinalBlock(secretSpendKey, 0, secretSpendKey.Length);

                if (accountType == AccountType.User)
                {
                    secretViewKeyEnc = aes.CreateEncryptor().TransformFinalBlock(secretViewKey, 0, secretViewKey.Length);
                }
                else
                {
                    secretViewKeyEnc = null;
                }
            }
        }

        private bool IsPasswordValid(Account account, string passphrase)
        {
            byte[] pwdHash = _hashCalculation.CalculateHash(Encoding.UTF8.GetBytes(passphrase));
            byte[] secretSpendKeyPwd = CryptoHelper.SumScalars(account.SecretSpendKey, pwdHash);
            byte[] secretViewKeyPwd = (account.AccountType == AccountType.User) ? CryptoHelper.SumScalars(account.SecretViewKey, pwdHash) : null;

            byte[] publicSpendKey = (account.AccountType == AccountType.User) ? CryptoHelper.GetPublicKey(secretSpendKeyPwd) : CryptoHelper.GetPublicKey(Ed25519.SecretKeyFromSeed(secretSpendKeyPwd));
            byte[] publicViewKey = (account.AccountType == AccountType.User) ? CryptoHelper.GetPublicKey(secretViewKeyPwd) : null;

            bool res = publicSpendKey.Equals32(account.PublicSpendKey) && (publicViewKey?.Equals32(account.PublicViewKey) ?? true);

            return res;
        }

        private Tuple<byte[], byte[]> GetSecretKeys(Account account, string passphrase)
        {
            using (var aes = Aes.Create())
            {
                aes.IV = _dataAccessService.GetAesInitializationVector();
                byte[] passphraseBytes = Encoding.ASCII.GetBytes(passphrase);
                aes.Key = SHA256.Create().ComputeHash(passphraseBytes);
                aes.Padding = PaddingMode.None;

                byte[] secretSpendKey = aes.CreateDecryptor().TransformFinalBlock(account.SecretSpendKey, 0, account.SecretSpendKey.Length);
                byte[] secretViewKey = aes.CreateDecryptor().TransformFinalBlock(account.SecretViewKey, 0, account.SecretViewKey.Length);

                return new Tuple<byte[], byte[]>(secretSpendKey, secretViewKey);
            }
        }

        private byte[] DecryptStateKeys(Account account, string passphrase)
        {
            byte[] secretSpendKey;

            using (var aes = Aes.Create())
            {
                aes.IV = _dataAccessService.GetAesInitializationVector();
                byte[] passphraseBytes = Encoding.ASCII.GetBytes(passphrase);
                aes.Key = SHA256.Create().ComputeHash(passphraseBytes);
                aes.Padding = PaddingMode.None;

                secretSpendKey = aes.CreateDecryptor().TransformFinalBlock(account.SecretSpendKey, 0, account.SecretSpendKey.Length);
            }

            byte[] publicSpendKeyBuf = CryptoHelper.GetPublicKey(Ed25519.SecretKeyFromSeed(secretSpendKey));

            bool res = publicSpendKeyBuf.Equals32(account.PublicSpendKey);

            return res ? secretSpendKey : null;
        }

        #endregion Private Functions	

        private class AuthenticationInput
        {
            public Account Account { get; set; }
            public string Password { get; set; }
        }
    }
}
