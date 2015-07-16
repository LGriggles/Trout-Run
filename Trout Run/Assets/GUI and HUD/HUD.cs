using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour
{
    // The reference screen dimensions. If the resolution is at this then scale is 1 and any GUI element position or sizes will be as specified (unscaled)
    private Vector2 refResolution = new Vector2(1280, 720);

    // For the weapon icons
    public Texture2D weaponIcons; // image of the weapon icons - should represent the wepaons in same order as they appear in weapons enum. "unarmed" should be bottom right
    Rect[] _weaponIconTexCoords; // uvs for each rectangle in the texture
    public int _weapTilesX = 2; // how many tiles across in texture?
    public int _weapTilesY = 2; // how many tiles down in textures?

    // Temp tex from colour function till have proper graphic
    Texture2D TexFromColour(Color colour)
    {
        Texture2D returnTex = new Texture2D(1, 1);
        returnTex.SetPixel(0, 0, colour);
        returnTex.Apply();
        return returnTex;
    }
    Texture2D _durabilityTex;

    // Stuff to do with player and weapon info
    int _playerCount = 1; // number of players
    int _currentWeaponIndex = 0; // just fo testing atm
    PlayerController[] _players; // ref to players


	// Use this for initialization
	void Awake () 
    {
        // Tex from colour for butters durability bar
        _durabilityTex = TexFromColour(Color.green);

        // Players
        _players = new PlayerController[_playerCount];
        _players[0] = GameObject.FindObjectOfType<PlayerController>().GetComponent<PlayerController>(); // this obviously won't work when add multi player
        if(_players[0] == null) Debug.LogError("WTF can't find player???");

        // Create uvs and calculate them
        _weaponIconTexCoords = new Rect[_weapTilesX * _weapTilesY];
        _weapTilesX = Mathf.Max(1, _weapTilesX); // can't be less than 1
        _weapTilesY = Mathf.Max(1, _weapTilesY);
        Vector2 tileSize = new Vector2(1/(float)_weapTilesX, 1/(float)_weapTilesY);

        for(int i = 0; i < _weaponIconTexCoords.Length; i++)
        {
            float xPos = (i%_weapTilesX); // find column
            float yPos = (Mathf.Floor(i/_weapTilesX)); // find row
            yPos = (_weapTilesY-1) - yPos; // invert y because stupid uvs go from bottom to top

            _weaponIconTexCoords[i].Set(xPos * tileSize.x, yPos * tileSize.y, tileSize.x, tileSize.y);
        }
	}

    float durability = 1;
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            _currentWeaponIndex += 1;
            if(_currentWeaponIndex == _weaponIconTexCoords.Length) _currentWeaponIndex = 0;
        }

        //temp need a durability for every player, just checking
        Weapon weapon = _players[0].weapon;
        if(weapon)
        {
            durability = (float)weapon.durability / (float)weapon.maxDurability;
            _currentWeaponIndex = (int)weapon.GetName();
        }
        else
        {
            durability = 0;
            _currentWeaponIndex = _weaponIconTexCoords.Length-1;
        }
    }
	
	// Draw all GUI stuff
	void OnGUI ()
    {
        Matrix4x4 oldmatrix = GUI.matrix; // store old matrix so we can set back after meddling
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width/refResolution.x, Screen.height/refResolution.y, 1));

        // Draw them tasty player infos
        for(int i = 0; i < _playerCount; i++)
        {
            Vector2 pos = new Vector2(40 + (i * 100), 40);
            DrawPlayerInfo(pos);
        }

        GUI.matrix = oldmatrix; // reset matrix in case something wants to use it unscaled (doubt it though...)
	}

    // Draw a single "player info" instance as position
    void DrawPlayerInfo(Vector2 pos)
    {
        // Draw weapon icon
        Rect iconPos = new Rect(pos.x, pos.y, 70, 70); // arbitary position for now
        GUI.DrawTextureWithTexCoords(iconPos, weaponIcons, _weaponIconTexCoords[_currentWeaponIndex]); // Draw weapon icon

        // Draw durability bar
        iconPos.width *= durability;
        iconPos.y += 74;
        iconPos.height = 10;
        GUI.DrawTexture(iconPos, _durabilityTex);



    }
}
