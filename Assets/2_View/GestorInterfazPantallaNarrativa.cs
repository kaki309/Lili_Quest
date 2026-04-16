using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GestorInterfazPantallaNarrativa : MonoBehaviour
{
    public static GestorInterfazPantallaNarrativa Instance;
    [SerializeField] GameObject _foto;
    [SerializeField] TMP_Text _subtitulo;
    [SerializeField] AudioSource _audioSource;
    [SerializeField] Image _laiaNarrator;

    [Header("Trivia")]
    [SerializeField] GameObject _canvasTrivia;
    [SerializeField] Image _laiaHappyInTrivia;

    void Awake()
    {
        Instance = this;
    }
    public GameObject Foto => _foto;
    public GameObject CanvasTrivia => _canvasTrivia;
    public TMP_Text Subtitulo => _subtitulo;
    public Image LaIaHappyInTrivia => _laiaHappyInTrivia;
    public AudioSource AudioSource => _audioSource;
    public Image LaiaNarrator => _laiaNarrator;
}
