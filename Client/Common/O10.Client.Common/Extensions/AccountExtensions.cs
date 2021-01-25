using System;
using System.Security.Cryptography;
using System.Text;
using O10.Core.ExtensionMethods;
using O10.Crypto.ConfidentialAssets;
using Account = O10.Client.DataLayer.Model.Account;

namespace O10.Client.Common.Extensions
{
    public static class AccountExtensions
    {
        public static Tuple<byte[], byte[]> DecryptStealthKeys(this Account account, string passphrase, byte[] aesInitializationVector)
        {
            byte[] publicSpendKeyBuf, publicViewKeyBuf;

            Tuple<byte[], byte[]> keys = GetSecretKeys(account, passphrase, aesInitializationVector);

            publicSpendKeyBuf = ConfidentialAssetsHelper.GetPublicKey(keys.Item1);
            publicViewKeyBuf = ConfidentialAssetsHelper.GetPublicKey(keys.Item2);

            bool res = publicSpendKeyBuf.Equals32(account.PublicSpendKey) && publicViewKeyBuf.Equals32(account.PublicViewKey);

            return res ? keys : null;
        }

        public static Tuple<byte[], byte[]> GetSecretKeys(Account account, string passphrase, byte[] aesInitializationVector)
        {
            using (var aes = Aes.Create())
            {
                aes.IV = aesInitializationVector;
                byte[] passphraseBytes = Encoding.ASCII.GetBytes(passphrase);
                aes.Key = SHA256.Create().ComputeHash(passphraseBytes);
                aes.Padding = PaddingMode.None;

                byte[] secretSpendKey = aes.CreateDecryptor().TransformFinalBlock(account.SecretSpendKey, 0, account.SecretSpendKey.Length);
                byte[] secretViewKey = aes.CreateDecryptor().TransformFinalBlock(account.SecretViewKey, 0, account.SecretViewKey.Length);

                return new Tuple<byte[], byte[]>(secretSpendKey, secretViewKey);
            }
        }

    }
}