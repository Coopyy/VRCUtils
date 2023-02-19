using CoopUtils;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using VRC;
using VRC.SDKBase;

[assembly: MelonInfo(typeof(Loader), "coopmod", "1", "Coopyy")]
[assembly: MelonColor(ConsoleColor.DarkCyan)]
namespace CoopUtils
{
    public class Loader : MelonMod
    {
        public static Loader instance;
        public static int startY = 450;
        public static Camera cam;
        public static int lastcount = 0;
        public static bool fly = false;
        public static Collider col = null;
        public static Vector3 backup;
        public override void OnEarlyInitializeMelon()
        {
            instance = this;
        }
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("guh");
        }
        public override void OnUpdate()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F))
                fly = !fly;
            var plr = VRCPlayer.field_Internal_Static_VRCPlayer_0;
            if (plr == null)
                return;
            if (cam == null || Networking.LocalPlayer == null || Networking.LocalPlayer.gameObject == null || Networking.LocalPlayer.gameObject.transform == null)
                return;
            if (col == null)
                col = Networking.LocalPlayer.gameObject.GetComponent<Collider>();
            if (col == null)
                return;
            if (Physics.gravity != Vector3.zero)
                backup = Physics.gravity;
            col.enabled = !fly;
            if (!fly)
            {
                Physics.gravity = backup;
                return;
            }
            Physics.gravity = Vector3.zero;
            Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));

            var speed = Input.GetKeyDown(KeyCode.LeftShift) ? 15 : 7.5f;

            if (Input.GetKey(KeyCode.W))
                plr.transform.position += cam.transform.forward * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.S))
                plr.transform.position -= cam.transform.forward * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.A))
                plr.transform.position -= cam.transform.right * speed * Time.deltaTime;
            if (Input.GetKey(KeyCode.D))
                plr.transform.position += cam.transform.right * speed * Time.deltaTime;
        }
        public override void OnGUI()
        {
            var list = PlayerManager.prop_PlayerManager_0?.field_Private_List_1_Player_0;
            if (list == null)
                return;
            if (VRCPlayerApi.AllPlayers == null)
                return;
            if (cam == null && Camera.main)
                cam = VRCVrCamera.field_Private_Static_VRCVrCamera_0.field_Public_Camera_0;
            int i = 0;
            string text = "Players: " + VRCPlayerApi.AllPlayers.Count;
            DrawOutlineLabel(new Vector2(10, startY + i++ * 18), Color.cyan, Color.black, text, text);
            foreach (VRC.Player item in list.ToArray())
            {
                if (item == null)
                    continue;
                var api = item._playerNet;
                if (api == null)
                    continue;
                var api1 = item.field_Private_APIUser_0;
                if (api1 == null)
                    continue;
                var fps = api.field_Private_Byte_0;
                var ping = api.field_Private_Int16_0;
                var name = api1.displayName;
                string platform = $"[{GetPlatform(item)}] ";
                string raw = platform + "[" + FPS(fps, false) + "fps] " + "[" + ping + "ms] " + name;
                string rich = platform + "[" + FPS(fps, true) + "fps] " + "[" + Ping(ping) + "ms] " + name;

                if (DrawOutlineLabel(new Vector2(10, startY + i++ * 18), Color.white, Color.black, rich, raw))
                    Networking.LocalPlayer.gameObject.transform.position = item.gameObject.transform.position;
            }

            if (VRCPlayerApi.AllPlayers.Count != lastcount)
                foreach (VRC.Player gameObject in list.ToArray())
                {
                    if (gameObject == null)
                        continue;
                    if (gameObject.transform == null)
                        continue;
                    Transform t = gameObject.transform.Find("SelectRegion");
                    if (t)
                    {
                        Renderer r = t.GetComponent<Renderer>();
                        if (r)
                        {
                            HighlightsFX fx = HighlightsFX.prop_HighlightsFX_0;
                            if (fx != null)
                                fx.Method_Public_Void_Renderer_Boolean_0(r, true);
                            lastcount = VRCPlayerApi.AllPlayers.Count;
                        }
                    }
                }
        }

        private static string FPS(byte f, bool col)
        {
            if (f == 0) return "\u221E";

            int fps = 1000 / f;
            if (!col)
                return fps.ToString();

            return (fps >= 60
            ? "<color=#0f0>"
            : fps >= 45
              ? "<color=#008000>"
              : fps >= 30
                ? "<color=#ffff00>"
                : fps >= 15
                  ? "<color=#ffa500>"
                  : "<color=#ff0000>")
            + fps + "</color>";
        }

        public static string GetPlatform(Player player)
        {
            if (player.prop_APIUser_0.last_platform == "standalonewindows")
                if (player.prop_VRCPlayerApi_0.IsUserInVR())
                    return "VR";
                else
                    return "PC";
            else
                return "Q";
        }

        public static string Ping(int ping) =>
            (ping <= 75
              ? "<color=#00ff00>"
              : ping <= 125
                ? "<color=#008000>"
                : ping <= 175
                  ? "<color=#ffff00>"
                  : ping <= 225
                    ? "<color=#ffa500>"
                    : "<color=#ff0000>") + ping + "</color>";
        public static bool DrawOutlineLabel(Vector2 rect, Color textcolor, Color outlinecolor, string text, string outlinetext = null)
        {
            GUIContent content = new GUIContent(text);
            if (outlinetext == null) outlinetext = text;
            GUIContent content1 = new GUIContent(outlinetext);
            GUIStyle style = GUI.skin.label;
            Vector2 size = style.CalcSize(content);
            GUI.color = Color.black;
            GUI.Label(new Rect((rect.x) + 1, rect.y + 1, size.x, size.y), content1);
            GUI.Label(new Rect((rect.x) - 1, rect.y - 1, size.x, size.y), content1);
            GUI.Label(new Rect((rect.x) + 1, rect.y - 1, size.x, size.y), content1);
            GUI.Label(new Rect((rect.x) - 1, rect.y + 1, size.x, size.y), content1);
            GUI.color = textcolor;
            return GUI.Button(new Rect(rect.x, rect.y, size.x, size.y), content, style: "label");
        }

        public static Color DoubleColorLerp(float percent, Color full, Color middle, Color empty)
        {
            if (percent < 0.5f)
                return Color.Lerp(empty, middle, percent * 2f);
            return Color.Lerp(middle, full, (percent - 0.5f) * 2f);
        }

    }
}
