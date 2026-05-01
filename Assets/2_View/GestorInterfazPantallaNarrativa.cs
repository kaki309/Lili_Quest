using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GestorInterfazPantallaNarrativa : MonoBehaviour
{
    public static GestorInterfazPantallaNarrativa Instance;
    [SerializeField] GameObject _foto;
    [SerializeField] AudioSource _audioSource;

    [Header("Trivia")]
    [SerializeField] GameObject _canvasTrivia;
    [SerializeField] Image _laiaHappyInTrivia;

    void Awake()
    {
        Instance = this;
    }
    public GameObject Foto => _foto;
    public GameObject CanvasTrivia => _canvasTrivia;
    public Image LaIaHappyInTrivia => _laiaHappyInTrivia;
    public AudioSource AudioSource => _audioSource;
}
