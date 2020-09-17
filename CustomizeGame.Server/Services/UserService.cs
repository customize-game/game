using MagicOnion;
using MagicOnion.Server;
using Microsoft.Extensions.Configuration.UserSecrets;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace CustomizeGame.Server.Services
{
    class UserService : ServiceBase<IUserService>, IUserService
    {
        //対象アルゴリズムの初期ベクター 複数の初期ベクターから選択して割り当てるなど？
        private static readonly string SEND_IV = "8pZqsaU9ZXH4SgFg";
        //対象アルゴリズムの共有鍵 ユーザー毎に生成して管理したいですね
        private static readonly string SEND_KEY = "i0gpPdAaMHjKXcJC";
        private AESCryption sendAES;

        //対象アルゴリズムの初期ベクター 
        private static readonly string SAVE_IV = "8pZqsaU9ZXH4SgFg";
        //対象アルゴリズムの共有鍵
        private static readonly string SAVE_KEY = "i0gpPdAaMHjKXcJC";
        private AESCryption saveAES;

        private static int TOKEN_LENGTH = 32;

        //とりあえずDBの代わり
        private static Dictionary<string, string> emailDictionary;
        private static Dictionary<string, string> userIdDictionary;

        public UserService() {
            sendAES = new AESCryption(SEND_IV, SEND_KEY);
            saveAES = new AESCryption(SAVE_IV, SAVE_KEY);

            if (emailDictionary == null)
            {
                emailDictionary = new Dictionary<string, string>();
            }

            if (userIdDictionary == null)
            {
                userIdDictionary = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// 発行した一意のUserIdかEmailで認証
        /// </summary>
        /// <param name="encUserIdOrEmail">encript UserID or Email</param>
        /// <param name="encPassword">encript Password</param>
        /// <param name="mode">0:userId/1:email</param>
        /// <returns>accessToken</returns>
        public UnaryResult<string> LoginCheck(string encUserIdOrEmail, string encPassword, int mode)
        {
            try
            {
                string userId = "";
                string email = "";
                string password = sendAES.Decrypt(encPassword);

                if (mode == 0)
                {
                    userId = sendAES.Decrypt(encUserIdOrEmail);
                    Logger.Debug($"{userId}:{password}");
                    if (userIdDictionary.ContainsKey(userId))
                    {
                        string checkPassword = saveAES.Decrypt(userIdDictionary[userId]);
                        if (checkPassword.Equals(password))
                        {
                            string accessToken = GetCsrfToken();
                            Logger.Debug("accessToken:" + accessToken);

                            return new UnaryResult<string>(sendAES.Encrypt(accessToken));
                        }
                    }
                }
                else
                {
                    email = sendAES.Decrypt(encUserIdOrEmail);
                    Logger.Debug($"{email}:{password}");
                    if (emailDictionary.ContainsKey(email))
                    {
                        string checkPassword = saveAES.Decrypt(emailDictionary[email]);
                        if (checkPassword.Equals(password))
                        {
                            string accessToken = GetCsrfToken();
                            Logger.Debug("accessToken:" + accessToken);

                            return new UnaryResult<string>(sendAES.Encrypt(accessToken));
                        }
                    }
                }
            }
            catch(Exception e) {
                Logger.Debug(e.Message);
            }

            return new UnaryResult<string>("");
        }

        /// <summary>
        /// ユーザー登録
        /// </summary>
        /// <param name="encUserName">encript username</param>
        /// <param name="encEmail">encript email</param>
        /// <param name="encPassword">encript password</param>
        /// <returns>userID</returns>
        public UnaryResult<string> AccountRegister(string encUserName, string encEmail, string encPassword)
        {
            try
            {
                string userName = sendAES.Decrypt(encUserName);
                string email = sendAES.Decrypt(encEmail);
                string password = sendAES.Decrypt(encPassword);

                string userId = Guid.NewGuid().ToString("N");

                while (userIdDictionary.ContainsKey(userId))
                {
                    userId = Guid.NewGuid().ToString("N");
                }

                //userID, userName, email, password あたりをDB登録
                //他必要ならTokenなど
                if (!emailDictionary.ContainsKey(email))
                {
                    userIdDictionary.Add(userId, saveAES.Encrypt(password));
                    emailDictionary.Add(email, saveAES.Encrypt(password));

                    return new UnaryResult<string>(sendAES.Encrypt(userId));
                }
 
            } catch(Exception e)
            {
                Logger.Debug(e.Message);
            }
            return new UnaryResult<string>("");
        }


        public static string GetCsrfToken()
        {
            byte[] token = new byte[TOKEN_LENGTH];
            RNGCryptoServiceProvider gen = new RNGCryptoServiceProvider();
            gen.GetBytes(token);
            StringBuilder buf = new StringBuilder();
            for (int i = 0; i < token.Length; i++)
            {
                buf.AppendFormat("{0:x2}", token[i]);
            }

            return buf.ToString();
        }
    }
}
