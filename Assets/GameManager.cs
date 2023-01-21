using UnityEngine;
using TMPro;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Encypt
    public TMP_InputField iPassword;
    public Animator animPassword;
    public TextMeshProUGUI tEncryptedPwd, tSecretKey, tEncryptClipboard;
    private bool canClipboardEncrypt = false;

    // Decrypt
    public TMP_InputField iEncryptedPwd, iSecretKey;
    public Animator animEncryptedPwd, animSecretKey;
    public TextMeshProUGUI tPassword, tDecryptClipboard;
    private bool canClipboardDecrypt = false;

    private void Awake()
    {
        Screen.SetResolution(1040, 585, false);
    }

    //AES - Encription 
    public void AESEncryption()
    {
        // Check if empty
        if(iPassword.text == "")
        {
            ErrorEncryption();
            return;
        }

        // Randomise KEY and IV
        string secretKey;
        using (RNGCryptoServiceProvider rng = new())
        {
            byte[] randomBytes = new byte[48]; // 32 bytes = 256 bits
            rng.GetBytes(randomBytes);
            secretKey = Convert.ToBase64String(randomBytes);
            secretKey = secretKey.Substring(0, 48);
        }

        // Provider
        AesCryptoServiceProvider AEScryptoProvider = new();
        AEScryptoProvider.BlockSize = 128;
        AEScryptoProvider.KeySize = 256;
        AEScryptoProvider.Key = Encoding.ASCII.GetBytes(secretKey.Substring(0, 32));
        AEScryptoProvider.IV = Encoding.ASCII.GetBytes(secretKey[32..]);
        AEScryptoProvider.Mode = CipherMode.CBC;
        AEScryptoProvider.Padding = PaddingMode.PKCS7;

        // Transform
        ICryptoTransform trnsfrm = AEScryptoProvider.CreateEncryptor(AEScryptoProvider.Key, AEScryptoProvider.IV);
        byte[] txtByteData = Encoding.ASCII.GetBytes(iPassword.text.Trim());
        byte[] result = trnsfrm.TransformFinalBlock(txtByteData, 0, txtByteData.Length);
        string encryptedPassword = Convert.ToBase64String(result);

        // Display
        tEncryptedPwd.text = encryptedPassword;
        tSecretKey.text = secretKey;

        // clipboard
        canClipboardEncrypt = true;
    }

    //AES -  Decryption
    public void AESDecryption()
    {
        // check if empty
        if (iEncryptedPwd.text == "" || iSecretKey.text == "")
        {
            ErrorDecryption();
            return;
        }

        // Provider
        AesCryptoServiceProvider AEScryptoProvider = new();
        AEScryptoProvider.BlockSize = 128;
        AEScryptoProvider.KeySize = 256;
        AEScryptoProvider.Key = Encoding.ASCII.GetBytes(iSecretKey.text.Trim().Substring(0, 32));
        AEScryptoProvider.IV = Encoding.ASCII.GetBytes(iSecretKey.text.Trim()[32..]);
        AEScryptoProvider.Mode = CipherMode.CBC;
        AEScryptoProvider.Padding = PaddingMode.PKCS7;

        // Transform
        ICryptoTransform trnsfrm = AEScryptoProvider.CreateDecryptor();
        byte[] txtByteData = Convert.FromBase64String(iEncryptedPwd.text.Trim());
        byte[] result = trnsfrm.TransformFinalBlock(txtByteData, 0, txtByteData.Length);
        string password = Encoding.ASCII.GetString(result);

        // display
        tPassword.text = password;

        // clipboard
        canClipboardDecrypt = true;
    }

    // Encrypt clipboard
    public void ClipboardEncrypt()
    {
        // Cannot
        if (!canClipboardEncrypt)
        {
            ErrorEncryption();
            return;
        }

        // Clipboard
        StringBuilder sb = new();
        sb.Append("Encrypted Password\n\t- ").Append(tEncryptedPwd.text).Append("\nSecret Key\n\t- " + tSecretKey.text);
        GUIUtility.systemCopyBuffer = sb.ToString();

        // Edit text + style of the button
        StopAllCoroutines();
        ResetClipboard(tDecryptClipboard);
        StartCoroutine(ClipboardCoroutine(tEncryptClipboard));
    }

    // Decrypt clipboard
    public void ClipboardDecrypt()
    {
        // Cannot
        if (!canClipboardDecrypt)
        {
            ErrorDecryption();
            return;
        }

        // Clipboard
        GUIUtility.systemCopyBuffer = tPassword.text;

        // Edit text + style of the button
        StopAllCoroutines();
        ResetClipboard(tEncryptClipboard);
        StartCoroutine(ClipboardCoroutine(tDecryptClipboard));
    }

    // Edit text + style of the button
    private IEnumerator ClipboardCoroutine(TextMeshProUGUI txtClipboard)
    {
        txtClipboard.text = "Copied";
        txtClipboard.fontStyle = FontStyles.Italic;
        yield return new WaitForSecondsRealtime(5);
        ResetClipboard(txtClipboard);
    }

    private void ResetClipboard(TextMeshProUGUI txtClipboard)
    {
        txtClipboard.text = "Copy";
        txtClipboard.fontStyle = FontStyles.Normal;
    }

    // Error
    private void ErrorEncryption()
    {
        animPassword.SetTrigger("error");
        tEncryptedPwd.text = "???";
        tSecretKey.text = "???";
        canClipboardEncrypt = false;
    }

    private void ErrorDecryption()
    {
        animEncryptedPwd.SetTrigger("error");
        animSecretKey.SetTrigger("error");
        tPassword.text = "???";
        canClipboardDecrypt = false;
    }
}
