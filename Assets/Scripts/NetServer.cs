using UnityEngine;
using System.Collections;

public class NetServer : MonoBehaviour {

	Vector2 scrollPosition;	//滚动视图位置
	string inputMessage = "";	//输入框信息
	string showMessage = "";	//显示信息
	bool netErrorShow = true;	//是否该通知用户，对方已下线

	// Use this for initialization
	void Start () {

	}
	
	void OnGUI(){

		//显示聊天框
		GUILayout.BeginArea (new Rect (5, 10, Screen.width, Screen.height));
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();	//对齐底部
		GUILayout.Box ( "\t\t<color=red><b> i am a legend.</b></color>", GUILayout.Width(180));
		scrollPosition = GUILayout.BeginScrollView (scrollPosition, GUILayout.Width (205), GUILayout.Height (210));
		GUILayout.Box (showMessage, GUILayout.Width (180));	//显示信息的Box
		GUILayout.EndScrollView ();
		GUILayout.BeginHorizontal ();
		GUI.SetNextControlName ("message_input");	//设置下个控件名字
		inputMessage = GUILayout.TextField (inputMessage,GUILayout.Width(150));	//发送信息
		if(Event.current.isKey && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter )&& GUI.GetNameOfFocusedControl() == "message_input"){//实现，按换行执行指令，用GetKeyDown(...)在TextField获得焦点的情况下没有作用
			if(Network.peerType != NetworkPeerType.Disconnected){
				//使用RPC发送内容
				networkView.RPC("ReceiveMessage", RPCMode.Others, inputMessage);
			}
			showMessage += "<color=red>I Say:</color> "+inputMessage+"\n";
			inputMessage = "";
			scrollPosition.y = float.MaxValue;	//滚动到最后
		}
		if(GUILayout.Button("发送",GUILayout.Width(50))){
			if(Network.peerType != NetworkPeerType.Disconnected){
				//使用RPC发送内容
				networkView.RPC("ReceiveMessage", RPCMode.Others, inputMessage);
			}
			showMessage += "<color=red>I Say:</color> "+inputMessage+"\n";
			inputMessage = "";
			scrollPosition.y = float.MaxValue;	//滚动到最后
		}
		GUILayout.EndHorizontal ();
		GUILayout.Space (20);
		GUILayout.EndVertical ();
		GUILayout.EndArea ();

		//网络状态
		switch(Network.peerType){
		case NetworkPeerType.Disconnected:
			if( GameManager.playWithWho == GameManager.PLAY_WITH_NET && netErrorShow ){
				showMessage += "<color=red>System: <b>对方网络已断开</b></color>\n";
				netErrorShow = false;
			}
			break;
		case NetworkPeerType.Server:
			//OnServer();
			break;
		case NetworkPeerType.Client:
			//OnClient();
			break;
		case NetworkPeerType.Connecting:
			break;
		}
		if( Network.connections.Length <= 0 && GameManager.playWithWho == GameManager.PLAY_WITH_NET && netErrorShow){
			showMessage += "<color=red>System: <b>对方网络已断开</b></color>\n";
			netErrorShow = false;
		}
	}
	
	// Update is called once per frame
	void Update () {

	}
	

	void OnServer(){

	}

	void OnClient(){

	}

	//接收其他人发送消息
	[RPC]
	void ReceiveMessage( string message ){
		showMessage += "<color=lime>Another Say:</color> "+message + "\n";
	}
}
