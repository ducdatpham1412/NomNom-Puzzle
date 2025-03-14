using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerManager : MonoBehaviour {
    [Header("GameObjects")]
    public EggvengerManager Manager;
    public Camera MainCamera;
    public PlayerMovement Movement;
    public PlayerSkill Skill;
    public PlayerVFX VFX;

    [Header("Sprites")]
    public SpriteRenderer PlayerRenderer;
    public SpriteRenderer GunRenderer;

    [Header("Audio")]
    public AudioSource PlayerAudio;

    [Header("Lights")]
    public Light2D FOVLight;
    public Light2D AimingLight;
    public Light2D BodyLight;


    [Header("Stats")]
    public float originalSpeed = 6.5f;
    public float buffSpeed = 0f;
    public List<float> speedRatios = new List<float>();
    public float currentSpeed = 6.5f;

    public string id;
    public int team;

    public int heal = 1000;
    public int shield = 0; // percentage: 0 - 100%
    public int eggs;

    public bool IsOwner; // TODO: Only for test, remove it
}
