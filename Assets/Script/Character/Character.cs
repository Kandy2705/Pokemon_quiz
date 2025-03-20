using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.Animation;

[System.Serializable]
public class Character
{
    public Sprite image;
    public string name;
    public string introduction;
    public int price;

    public bool isPurchased;
    public bool isSelected;

    public SpriteLibraryAsset spriteLibraryAsset;
}
