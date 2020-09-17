using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSceneManager : MonoBehaviour
{
    #region Unityのインスペクターで初期設定
    [SerializeField] private SceneField nextScene = default;

    [SerializeField] private GameObject initPanel = default;
    [SerializeField] private GameObject registerPanel = default;
    [SerializeField] private GameObject loginPanel = default;

    [SerializeField] private Text messageText = default;

    [SerializeField] private Button startButton = default;
    [SerializeField] private Button startOtherLoginButton = default;
    [SerializeField] private Button startNewAccountButton = default;

    [SerializeField] private Button registerButton = default;
    [SerializeField] private Button registerLoginButton = default;
    
    [SerializeField] private Button loginNewAccountButton = default;
    [SerializeField] private Button loginButton = default;

    [SerializeField] private InputField registerUsernameInput = default;
    [SerializeField] private InputField registerEmailInput = default;
    [SerializeField] private InputField registerPasswordInput = default;

    [SerializeField] private InputField loginEmailInput = default;
    [SerializeField] private InputField loginPasswordInput = default;
    #endregion

    private AuthManager authManager;
    private UIFocusManager uIFocusManager;

    #region 初期処理

    private void Awake()
    {
        initPanel.SetActive(true);
        registerPanel.SetActive(false);
        loginPanel.SetActive(false);
    }

    private void Start()
    {
        authManager = GameObject.FindGameObjectWithTag("AuthManager")
                                .GetComponent<AuthManager>();
        uIFocusManager = GameObject.Find("LoginUI").GetComponent<UIFocusManager>();
        messageText.text = "";

        //buttonイベント購読設定
        SubscribeStartButton();
        SubscribeStartOtherLoginButton();
        SubscribeStartNewAccountButton();

        SubscribeRegisterButton();
        SubscribeRegisterLoginButton();

        SubscribeLoginButton();
        SubscribeNewAccountButton();

        //入力フィールドのイベント購読設定
        SubscribeRegisterInputField();
        SubscribeLoginInputField();
    }

    #endregion

    #region ボタン押下処理
    public async void OnClickStart() {
        try
        {
            SetLoginMenuMessage(initPanel, "ログインチェック...");
            AuthManager.AuthRes res = await authManager.AutoLoginCheck();

            switch (res)
            {
                case AuthManager.AuthRes.NoAccount:
                    DisplayRegisterForm();
                    break;
                case AuthManager.AuthRes.Success:
                    MoveMenuScene();
                    break;
                case AuthManager.AuthRes.Failed:
                default:
                    DisplayLoginForm();
                    break;
            }
        }
        catch (Exception e)
        {
            SetLoginMenuMessage(initPanel, "サーバーへの接続失敗");
            Debug.Log(e);
        }
        finally {
            startButton.interactable = true;
        }
    }

    public async void OnClickLogin() {
        try
        {
            string email = loginEmailInput.text;
            string password = loginPasswordInput.text;

            SetLoginMenuMessage(loginPanel, "");
            AuthManager.AuthRes res = await authManager.LoginCheck(email, password);

            switch (res)
            {
                case AuthManager.AuthRes.Success:
                    MoveMenuScene();
                    break;
                case AuthManager.AuthRes.NoAccount:
                case AuthManager.AuthRes.Failed:
                default:
                    SetLoginMenuMessage(loginPanel, "ログイン失敗：ユーザー名またはパスワードが違います");
                    break;
            }
        }
        catch (Exception e)
        {
            SetLoginMenuMessage(loginPanel, "サーバーへの接続失敗");
            Debug.Log(e);
        }
        finally {
            loginPasswordInput.ActivateInputField();
        }

    }

    public async void OnClickRegister()
    {
        try
        {
            string username = registerUsernameInput.text;
            string email = registerEmailInput.text;
            string password = registerPasswordInput.text;

            SetLoginMenuMessage(registerPanel, "");
            AuthManager.AuthRes res = await authManager.AccountRegister(username, email, password);

            switch (res)
            {
                case AuthManager.AuthRes.Success:
                    DisplayLoginForm();
                    loginEmailInput.text = email;
                    break;
                case AuthManager.AuthRes.Failed:
                default:
                    SetLoginMenuMessage(registerPanel, "アカウント作成失敗");
                    break;
            }
        }
        catch (Exception e)
        {
            SetLoginMenuMessage(loginPanel, "サーバーへの接続失敗");
            Debug.Log(e);
        }
        finally
        {
            loginPasswordInput.ActivateInputField();
        }
    }

    #endregion

    #region 表示処理

    private void DisplayRegisterForm() {
        messageText.text = "";
        registerButton.interactable = false;
        initPanel.SetActive(false);
        registerPanel.SetActive(true);
        loginPanel.SetActive(false);

        uIFocusManager.updateSelectableList();
    }

    private void DisplayLoginForm() {
        messageText.text = "";
        loginButton.interactable = false;
        initPanel.SetActive(false);
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);

        uIFocusManager.updateSelectableList();
    }

    #endregion

    #region 入力チェック処理

    private bool CheckUsername(string text) {
        string checkPattern = @"^[0-9a-zA-Z_\.\-]{6,18}$";

        return Regex.IsMatch(text, checkPattern);
    }

    private bool CheckEmail(string text) {

        return Regex.IsMatch(text,
                @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
    }

    private bool CheckPassword(string text) {
        string checkPattern = @"^(?=.*?[a-z])(?=.*?[A-Z])(?=.*?\d)(?=.*?[!-~])[a-zA-Z\d!-~]{8,24}$";

        return Regex.IsMatch(text, checkPattern);
    }

    #endregion

    #region Scene 遷移

    private void MoveMenuScene() {
        SceneManager.LoadSceneAsync(nextScene.BuildIndex);
    }

    #endregion

    #region イベント購読設定 UniRx

    private void SubscribeStartButton() {
        startButton.OnClickAsObservable()
            .Where(_ => startButton.interactable)
            .Subscribe(_ =>
            {
                startButton.interactable = false;
                OnClickStart();
            })
            .AddTo(gameObject);
    }

    private void SubscribeStartOtherLoginButton()
    {
        startOtherLoginButton.OnClickAsObservable()
            .Where(_ => startOtherLoginButton.interactable)
            .Subscribe(_ =>
            {
                DisplayLoginForm();
            })
            .AddTo(gameObject);
    }

    private void SubscribeStartNewAccountButton()
    {
        startNewAccountButton.OnClickAsObservable()
            .Where(_ => startNewAccountButton.interactable)
            .Subscribe(_ =>
            {
                DisplayRegisterForm();
            })
            .AddTo(gameObject);
    }

    private void SubscribeLoginButton() {
        loginButton.OnClickAsObservable()
            .Where(_ => loginButton.interactable)
            .Subscribe(_ =>
            {
                loginButton.interactable = false;
                OnClickLogin();
            })
            .AddTo(gameObject);
    }

    private void SubscribeNewAccountButton() {
        loginNewAccountButton.OnClickAsObservable()
            .Where(_ => loginNewAccountButton.interactable)
            .Subscribe(_ =>
            {
                DisplayRegisterForm();
            })
            .AddTo(gameObject);
    }

    private void SubscribeRegisterLoginButton()
    {
        registerLoginButton.OnClickAsObservable()
            .Where(_ => registerLoginButton.interactable)
            .Subscribe(_ =>
            {
                DisplayLoginForm();
            })
            .AddTo(gameObject);
    }

    private void SubscribeRegisterButton()
    {
        registerButton.OnClickAsObservable()
            .Where(_ => registerButton.interactable)
            .Subscribe(_ =>
            {
                registerButton.interactable = false;
                OnClickRegister();
            })
            .AddTo(gameObject);
    }

    private void SubscribeLoginInputField() {
        bool checkEmail = false;
        bool checkPassword = false;

        loginEmailInput.OnEndEditAsObservable()
            .Subscribe(msg =>
            {
                messageText.text = "";
                checkEmail = CheckEmail(msg);
                if (!checkEmail)
                {
                    SetLoginMenuMessage(loginPanel, "メールアドレスが正しくありません");
                }
                CheckResultSetInputField(loginEmailInput, checkEmail);

                if (checkEmail && checkPassword)
                {
                    loginButton.interactable = true;
                }
                else {
                    loginButton.interactable = false;
                }
            })
            .AddTo(gameObject);

        loginPasswordInput.OnEndEditAsObservable()
            .Subscribe(msg =>
            {
                messageText.text = "";
                checkPassword = CheckPassword(msg);
                if (!checkPassword)
                {
                    SetLoginMenuMessage(loginPanel, "パスワードが正しくありません");
                }
                CheckResultSetInputField(loginPasswordInput, checkPassword);

                if (checkEmail && checkPassword)
                {
                    loginButton.interactable = true;
                }
                else
                {
                    loginButton.interactable = false;
                }
            })
            .AddTo(gameObject);

    }

    private void SubscribeRegisterInputField()
    {
        bool checkUsername = false;
        bool checkEmail = false;
        bool checkPassword = false;

        registerUsernameInput.OnEndEditAsObservable()
            .Subscribe(msg =>
            {
                messageText.text = "";
                checkUsername = CheckUsername(msg);
                if (!checkUsername)
                {
                    SetLoginMenuMessage(registerPanel, "ユーザー名が正しくありません");
                }
                CheckResultSetInputField(registerUsernameInput, checkUsername);

                if (checkUsername && checkEmail && checkPassword)
                {
                    registerButton.interactable = true;
                }
                else {
                    registerButton.interactable = false;
                }
            })
            .AddTo(gameObject);

        registerEmailInput.OnEndEditAsObservable()
            .Subscribe(msg =>
            {
                messageText.text = "";
                checkEmail = CheckEmail(msg);
                if (!checkEmail)
                {
                    SetLoginMenuMessage(registerPanel, "メールアドレスの入力が正しくありません");
                }
                CheckResultSetInputField(registerEmailInput, checkEmail);

                if (checkUsername && checkEmail && checkPassword)
                {
                    registerButton.interactable = true;
                }
                else
                {
                    registerButton.interactable = false;
                }
            })
            .AddTo(gameObject);

        registerPasswordInput.OnEndEditAsObservable()
            .Subscribe(msg =>
            {
                messageText.text = "";
                checkPassword = CheckPassword(msg);
                if (!checkPassword)
                {
                    SetLoginMenuMessage(registerPanel, "半角英数字大文字小文字を含む8～24桁");
                }
                CheckResultSetInputField(registerPasswordInput, checkPassword);

                if (checkUsername && checkEmail && checkPassword)
                {
                    registerButton.interactable = true;
                }
                else
                {
                    registerButton.interactable = false;
                }
            })
            .AddTo(gameObject);

    }

    #endregion

    private void SetLoginMenuMessage(GameObject panel, string msg) {
        if (panel.activeSelf)
        {
            messageText.text = msg;
        }
    }

    private void CheckResultSetInputField(InputField input, bool result) {
        input.GetComponentsInChildren<Text>(true)
            .ToList()
            .ForEach(t =>
            {
                if (t.gameObject.name.Equals("OKText"))
                {
                    t.gameObject.SetActive(result);
                }
                else if(t.gameObject.name.Equals("NGText"))
                {
                    t.gameObject.SetActive(!result);
                }
            });
    }
}
