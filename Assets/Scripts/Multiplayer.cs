using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Multiplayer : MonoBehaviour
{
    public GameObject prePlayer;
    public List<InputDevice> activatedDevices = new List<InputDevice>();
    public List<PlayerInput> playerInputs = new List<PlayerInput>();

    public void OnSetup(InputAction.CallbackContext ctx)
    {
        Debug.Log(InputSystem.devices.Count);
        GetComponent<PlayerInput>().SwitchCurrentControlScheme(InputSystem.devices.ToArray());
        Debug.Log(GetComponent<PlayerInput>().currentControlScheme);
        Debug.Log("TEST");
        if (activatedDevices.Contains(ctx.control.device)) return;
        PlayerInput p;
        if (ctx.control.device == Keyboard.current.device) p = PlayerInput.Instantiate(prePlayer, playerIndex: playerInputs.Count, pairWithDevices: new[] { Keyboard.current.device, Mouse.current.device });
        else p = PlayerInput.Instantiate(prePlayer, playerIndex: playerInputs.Count, pairWithDevice: ctx.control.device);
        //p.gameObject.GetComponent<ThirdPersonMovement>().;
        activatedDevices.Add(ctx.control.device);
        float rows = 1, cols = 1;
        //for (int i = 20; i < 30; i++) if (i != 20 + playerInputs.Count) p.GetComponentInChildren<Camera>().cullingMask &= ~i;
        //p.GetComponentInChildren<Camera>().gameObject.layer = 20 + playerInputs.Count;
        //p.GetComponentInChildren<Cinemachine.CinemachineFreeLook>().gameObject.layer = 20 + playerInputs.Count;
        p.GetComponentInChildren<Cinemachine.CinemachineInputProvider>().PlayerIndex = p.playerIndex;
        while (rows * cols < activatedDevices.Count) if (rows < cols) rows++; else cols++;
        playerInputs.Add(p);
        for (int i = 0; i < playerInputs.Count; i++)
        {
            float row = Mathf.FloorToInt(i / cols) / rows;
            float col = i % cols / cols;
            float h = 1f / rows;
            float w = (i < playerInputs.Count - 1) ? 1f / cols : 1f / cols * (cols - col);
            playerInputs[i].GetComponentInChildren<Camera>().rect = new Rect(col, row, w, h);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
