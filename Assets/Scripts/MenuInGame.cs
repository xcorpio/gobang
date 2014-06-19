using UnityEngine;
using System.Collections;

public class MenuInGame : MonoBehaviour {

	Rect windowRect = new Rect((Screen.width-300)/2,(Screen.height-200)/2,300,200);	//帮助窗口
	bool isShowHelpWindow = false;	//是否显示帮助窗口


	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI(){
		GUILayout.BeginArea (new Rect (10, 10, 110, 100));
		if(GUILayout.Button("帮助")){
			isShowHelpWindow = true;
		}
		if(isShowHelpWindow){
			windowRect = GUILayout.Window(1,windowRect, WindowFunc,"<color=red>X Help U</color>");
		}
		if( GUILayout.Button ("返回主菜单") ){
			Application.LoadLevel("menu");	//返回主菜单
			if(Network.peerType != NetworkPeerType.Disconnected ){//断开网络
				Network.Disconnect();
			}
		}
		GUILayout.Box ("剩余悔棋次数:" + GameManager.canBackSteps);
		if( GUILayout.Button ("我要悔棋") ){
			SendMessage("Rollback");		//调用回退函数
		}
		GUILayout.EndArea ();
	}

	void WindowFunc( int windowID ){
		GUILayout.Box ("\n鼠标左键下棋.\n右键旋转棋盘.\n中键移动.\n滚轮缩放.\n左上角菜单.\n左下角聊天窗口.\n" +
			"<color=lime>Author:mr.zheng\nEmail:xcorpio@qq.com\nPhone:15109221400</color>");
		if( GUILayout.Button ("确定") ){
			isShowHelpWindow = false;
		}
		GUI.DragWindow ();	//拖动窗口
	}
}
