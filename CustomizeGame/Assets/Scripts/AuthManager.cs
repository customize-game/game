using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager instance;

    //認証用のAccessToken
    private static string accessToken;

    //対象アルゴリズムの初期ベクター 複数の初期ベクターから選択して割り当てるなど？
    private static readonly string SEND_IV = "8pZqsaU9ZXH4SgFg";
    //対象アルゴリズムの共有鍵 ユーザー毎に生成して管理したいですね
    private static readonly string SEND_KEY = "i0gpPdAaMHjKXcJC";
    private AESCryption sendAES;

    private IUserService userService;
    private BaseNetworkManager baseNetworkManager;


    //対象アルゴリズムの初期ベクター 保存用
    private static readonly string SAVE_IV = "Mc6vA7WDkVBB3myG";
    //対象アルゴリズムの共有鍵 保存用
    private static readonly string SAVE_KEY = "GZUKtVWwnXTUfy0J";
    private AESCryption saveAES;

    public AuthManager() {
        instance = null;
        sendAES = new AESCryption(SEND_IV, SEND_KEY);
        saveAES = new AESCryption(SAVE_IV, SAVE_KEY);
        accessToken = null;
    }

    private void Awake()
    {
        //Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        baseNetworkManager = GameObject.FindWithTag("BaseNetworkManager").GetComponent<BaseNetworkManager>();

        //AuthTest();
    }

    private async void AuthTest() {

        Debug.Log("accessToken:" + accessToken);

        AuthRes res1 = await AccountRegister("testae", "testae@test.com", "123daeE#");
        Debug.Log("res1:"+res1);

        AuthRes res2 = await AutoLoginCheck();
        Debug.Log("res2"+res2);

        Debug.Log("accessToken:" + accessToken);

        AuthRes res3 = await LoginCheck("testae@test.com", "123daeE#");
        Debug.Log("res3" + res3);

        Debug.Log("accessToken:" + accessToken);

        AuthRes res4 = await LoginCheck("testae123@test.com", "123daeE#");
        Debug.Log("res4" + res4);

        Debug.Log("accessToken:" + accessToken);

        AuthRes res5 = await LoginCheck("testae@test.com", "1gre23daeE#");
        Debug.Log("res5" + res5);

        Debug.Log("accessToken:" + accessToken);
    }

    public enum AuthRes {
        Success,
        Failed,
        NoAccount
    }

    private static readonly string PREF_AUTH_USERID = "AuthUserId";
    private static readonly string PREF_AUTH_PASSWORD = "AuthPassword";

    public async Task<AuthRes> AccountRegister(string userName, string email, string password) {

        if (userService == null)
        {
            userService = MagicOnionClient.Create<IUserService>(baseNetworkManager.ConnectChannel());
        }

        string encUserId = await userService.AccountRegister(sendAES.Encrypt(userName), sendAES.Encrypt(email), sendAES.Encrypt(password));

        if (encUserId == "") {
            return AuthRes.Failed;
        }
        
        string userId = sendAES.Decrypt(encUserId);
        
        //送信とは別のキーで暗号化して保管
        PlayerPrefs.SetString(PREF_AUTH_USERID, saveAES.Encrypt(userId));
        PlayerPrefs.SetString(PREF_AUTH_PASSWORD, saveAES.Encrypt(password));

        return AuthRes.Success;
    }

    public enum CheckMode {
        UserId = 0,
        Email = 1
    }

    public async Task<AuthRes> LoginCheck(string email, string password)
    {
        if (userService == null)
        {
            userService = MagicOnionClient.Create<IUserService>(baseNetworkManager.ConnectChannel());
        }
        accessToken = await userService.LoginCheck(sendAES.Encrypt(email)
                                , sendAES.Encrypt(password), (int)CheckMode.Email);

        if (accessToken == "")
        {
            return AuthRes.Failed;
        }

        return AuthRes.Success;
    }

    public async Task<AuthRes> AutoLoginCheck() {
        if (userService == null)
        {
            userService = MagicOnionClient.Create<IUserService>(baseNetworkManager.ConnectChannel());
        }

        if (PlayerPrefs.HasKey(PREF_AUTH_USERID) && PlayerPrefs.HasKey(PREF_AUTH_PASSWORD))
        {
            string userId = saveAES.Decrypt(PlayerPrefs.GetString(PREF_AUTH_USERID));
            string password = saveAES.Decrypt(PlayerPrefs.GetString(PREF_AUTH_PASSWORD));
            accessToken = await userService.LoginCheck(sendAES.Encrypt(userId), sendAES.Encrypt(password), (int)CheckMode.UserId);

            if (accessToken == "") {
                return AuthRes.Failed;
            }
        }
        else {
            return AuthRes.NoAccount;
        }

        return AuthRes.Success;
    }

    public string getAccessToken() {
        return accessToken;
    }
}
