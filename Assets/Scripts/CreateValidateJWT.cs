using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.OpenSsl;
using System.Diagnostics;
using Org.BouncyCastle.Crypto.Paddings;

namespace CreateValidateJWT
{
    class Program
    {
        private static string ServiceAccountEmail = Config.GetEmail();
        private string privateKey = Config.GetKey();

        public string GenerateJWTToken(string uid)
        {
            var payload = new Dictionary<string, object>()
            {
                { "iss",  ServiceAccountEmail},
                { "sub",  ServiceAccountEmail},
                { "aud", "https://identitytoolkit.googleapis.com/google.identity.identitytoolkit.v1.IdentityToolkit" },
                { "iat", (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds },
                { "exp", (Int32)(DateTime.UtcNow.AddHours(1).Subtract(new DateTime(1970, 1, 1))).TotalSeconds},
                { "uid", uid }
            };
            RsaPrivateCrtKeyParameters rsaPrivateCrtKeyParameters1;
            var keyBytes = Convert.FromBase64String(privateKey);
            // 바이트 배열로 변환
            var asymmetricKeyParameter = PrivateKeyFactory.CreateKey(keyBytes);// 바이트 배열 -> 키로 변환

            rsaPrivateCrtKeyParameters1 = (RsaPrivateCrtKeyParameters)asymmetricKeyParameter; // RSAPrivateCryKey로 변환

            RSAParameters r = DotNetUtilities.ToRSAParameters(rsaPrivateCrtKeyParameters1); // RSAParameters로 변환

            var encoder = GetRS256JWTEncoder(r);
            var header = new Dictionary<string, object>
            {
                { "header_key", "bd6143cfa8f5c301035762ef9c840ae5fd5d11e3" }
            };
            var token = encoder.Encode(header, payload, new byte[0]); // header, payload, signing key
            return token;
        }
        private static IJwtEncoder GetRS256JWTEncoder(RSAParameters rsaParams) // Encoder 생성하는 함수 (매개변수 : RSAParameters)
        {
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(rsaParams);

            var algorithm = new RS256Algorithm(csp, csp);
            var serializer = new JsonNetSerializer();
            var urlEncoder = new JwtBase64UrlEncoder();
            var encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            return encoder;
        }

    }
}
