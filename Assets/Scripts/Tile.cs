using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color baseColor, offsetColor;
    [SerializeField] private SpriteRenderer _renderer;
    public Vector2Int origin;
    
    public void Init(bool isOffset){
        _renderer.color = isOffset ? offsetColor : baseColor;
    }
}
