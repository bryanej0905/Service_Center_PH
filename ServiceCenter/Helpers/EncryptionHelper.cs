using System;
using System.Security.Cryptography;
using System.Text;

namespace ServiceCenter.Helpers
{
    public static class EncryptionHelper
    {
        /// <summary>
        /// Cifra un texto plano usando DPAPI (por usuario) y devuelve un Base64.
        /// </summary>
        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            var data = Encoding.UTF8.GetBytes(plainText);
            var encrypted = ProtectedData.Protect(
                data,
                null,
                DataProtectionScope.CurrentUser
            );
            return Convert.ToBase64String(encrypted);
        }

        /// <summary>
        /// Descifra un Base64 cifrado con DPAPI. Si el texto no es Base64 válido o la descifrado falla,
        /// devuelve el texto de entrada sin modificar.
        /// </summary>
        public static string Decrypt(string encryptedText)
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            try
            {
                var encryptedData = Convert.FromBase64String(encryptedText);
                var decryptedBytes = ProtectedData.Unprotect(
                    encryptedData,
                    null,
                    DataProtectionScope.CurrentUser
                );
                return Encoding.UTF8.GetString(decryptedBytes);
            }
            catch (FormatException)
            {
                // No es Base64: devolver tal cual
                return encryptedText;
            }
            catch (CryptographicException)
            {
                // Falla DPAPI: devolver tal cual
                return encryptedText;
            }
        }
    }
}
