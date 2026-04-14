using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkStatus :
    MonoBehaviour
{
    public TMP_Text statusText;
    void Update()
    {
        if
            (Application.internetReachability ==
             NetworkReachability.NotReachable)
        {
            statusText.text = "Sem conexão com a internet!!";
            statusText.color =
                Color.red;
        }
        else
        {
            statusText.text =
                "Conectado";
            statusText.color =
                Color.green;
        }
    }
}