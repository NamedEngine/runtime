using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LogicExceptionHandler : MonoBehaviour {
    [SerializeField] GameObject errorBox;
    [SerializeField] TextMeshProUGUI errorTextField;

    Button _quitButton;
    
    void Start() {
        errorBox.SetActive(false);
        _quitButton = errorBox.transform.GetComponentInChildren<Button>();
        _quitButton.onClick.AddListener(Quitter.Quit);
    }

    public void DisplayError(string message) {
        errorTextField.text = message;
        errorBox.SetActive(true);
    }
}
