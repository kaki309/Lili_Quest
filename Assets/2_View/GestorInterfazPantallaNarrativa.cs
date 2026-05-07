using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GestorInterfazPantallaNarrativa : MonoBehaviour
{
    public static GestorInterfazPantallaNarrativa Instance;
    [SerializeField] GameObject _foto;

    [Header("Trivia")]
    [SerializeField] GameObject _canvasTrivia;
    [SerializeField] Image _laiaInTrivia;

    void Awake()
    {
        Instance = this;
    }
    public GameObject Foto => _foto;
    public GameObject CanvasTrivia => _canvasTrivia;
    public Image LaIaInTrivia => _laiaInTrivia;
}
