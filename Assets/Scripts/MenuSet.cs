using UnityEngine;
using System.Collections;
using System;


/**
 * A) 能够人机对弈，可选择人先走或人后走，具体规则参考通用的五子棋规则；
 * B) 能够悔棋，最多连续悔5步棋；
 * C) 设置游戏的难易程度；
 * D) 可选功能，可联网2人对战。
*/

public class MenuSet : MonoBehaviour {

	public AudioSource music;	//播放音乐
	bool isPlayMusic = true;	//是否播放
	public Texture2D bg;	//背景贴图
	string[] toolbarStrings = { "人机对战", "网络对战" ,"自己玩耍"};	//工具条按钮显示字符串
	int toolbarSelectID;		//选择了工具条那个按钮

	bool isMeGoFirst = true;		//自己先走
	bool isAIGoFirst = false;		//AI先走
	int whoGoFirst;					//谁先走, 1 自己 ,2 AI .用Toggle 开关 实现单选功能

	bool isNormalLevel = true;		//普通难度
	bool isHardlevel = false;		//困难难度
	bool isExpertLevel = false;			//专家难度
	int level;						//难度级别, 1 普通 ,2 困难 ,3 专家  同样为实现单选功能

	bool isAsServer = true;			//作为服务端
	bool isAsClient = false;		//作为客户端
	int peerType;					//1 作为服务端,2 作为客户端
	string serverPort = "8888";		//服务端端口
	string serverIP = "127.0.0.1";	//服务端IP, "127.1.1.1"居然连接不上，不明原因
	string connectInfo;				//连接显示信息
	float connectTime = 0;			//客户端连接时间
	
	float oldTime;		//计时
	int frame;			//记录字符串状态，动态文字

	int canBackSteps = 5;			//连续悔几步棋

	bool isNotStyled = true;		//是否还没设置样式
	GUIStyle boxStyle; 				//box样式

	void Start(){
		Screen.showCursor = true;	//显示光标
		music.Play ();				//播放声音
	}

	void OnGUI(){

		GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), bg, ScaleMode.StretchToFill);

		if(isNotStyled){
			//会影响到全局
			boxStyle = GUI.skin.box;	//所有Box都是一个style?
			boxStyle.alignment = TextAnchor.UpperLeft;	//左上对齐
			boxStyle.richText = true;		//富文本
			boxStyle.wordWrap = true;		//自动换行
			boxStyle.normal.textColor = Color.magenta;	//字体颜色
			boxStyle.stretchHeight = true;	//高度自动延伸
			isNotStyled = false;
		}

		GUILayout.BeginArea (new Rect (20, 20, Screen.width - 30, 100));
		GUILayout.BeginHorizontal ();
		toolbarSelectID = GUILayout.Toolbar (toolbarSelectID, toolbarStrings);
		GUILayout.FlexibleSpace ();
		isPlayMusic = GUILayout.Toggle (isPlayMusic, "<color=yellow><b>声音开关</b></color>");
		if(isPlayMusic){
			if(!music.isPlaying){
				music.Play();
			}
		}else{
			if(music.isPlaying){
				music.Pause();				//关闭声音
			}
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndArea ();

		switch(toolbarSelectID){
		case 0:	//人机对战

			if(Network.peerType != NetworkPeerType.Disconnected){
				Network.Disconnect();	//断开网络
			}

			//谁先走
			GUILayout.BeginArea(new Rect(20,80,100,100));
			isMeGoFirst = GUILayout.Toggle( isMeGoFirst, "我先走");
			if(isMeGoFirst){
				isAIGoFirst =false;
				whoGoFirst = 1;
			}
			isAIGoFirst = GUILayout.Toggle( isAIGoFirst, "AI先走");
			if(isAIGoFirst){
				//isMeGoFirst = false;
				whoGoFirst =2;
			}
			if(whoGoFirst == 1){
				isMeGoFirst = true;
				isAIGoFirst = false;
			}else if( whoGoFirst == 2){
				isMeGoFirst = false;
				isAIGoFirst = true;
			}
			//难度级别
			GUILayout.EndArea();
			GUILayout.BeginArea(new Rect(140,80,100,150));
			isNormalLevel = GUILayout.Toggle(isNormalLevel, "普通");
			if(isNormalLevel){
				isHardlevel =false;
				isExpertLevel = false;
				level = 1;
			}
			isHardlevel = GUILayout.Toggle( isHardlevel, "困难");
			if(isHardlevel){
				isNormalLevel  = false;
				isExpertLevel = false;
				level = 2;
			}
			isExpertLevel = GUILayout.Toggle( isExpertLevel, "专家");
			if(isExpertLevel){
				level = 3; 
			}
			if(level == 1){
				isNormalLevel = true;
				isHardlevel = false;
				isExpertLevel = false;
			}else if( level == 2){
				isNormalLevel = false;
				isHardlevel = true;
				isExpertLevel = false;
			}else if( level == 3){
				isNormalLevel = false;
				isHardlevel = false;
				isExpertLevel = true;
			}
			GUILayout.EndArea();
			break;
		case 1:	//网络对战
			GUILayout.BeginArea( new Rect(20,60, 600,150));

			GUILayout.BeginHorizontal(GUILayout.Width(350),GUILayout.Height(30));	//水平布局
			isAsServer = GUILayout.Toggle(isAsServer, "作为服务端",GUILayout.Width(100));
			if(isAsServer){
				GUILayout.Box("服务端端口: ",GUILayout.Width(100));
				serverPort = GUILayout.TextField( serverPort, GUILayout.Width(50) );
				if ( GUILayout.Button( "开启服务器", GUILayout.Width(100) ) ){
					StartServer();
				}
				isAsClient = false;			//作为客户端选项为假，只能做一种类型，为实现单选
				peerType = 1;
				if( Network.peerType == NetworkPeerType.Connecting){	//连接中
					Network.Disconnect();	//断开连接
					connectInfo = "";		//清空字符串
				}
			}
			GUILayout.EndHorizontal();		//结束水平布局

			GUILayout.BeginHorizontal(GUILayout.Width(550),GUILayout.Height(30));
			isAsClient = GUILayout.Toggle( isAsClient,"作为客户端",GUILayout.Width(100));
			if(isAsClient){
				GUILayout.Box("服务端IP: ", GUILayout.Width(100));
				serverIP = GUILayout.TextField(serverIP, GUILayout.Width(100));
				GUILayout.Box("服务端端口: ", GUILayout.Width(100));
				serverPort = GUILayout.TextField(serverPort, GUILayout.Width(50));
				if(GUILayout.Button("连接服务器" , GUILayout.Width(100))){
					ConnectServer();
				}
				isAsServer = false;
				peerType = 2;
				if(Network.peerType == NetworkPeerType.Server){	//断开连接
					Network.Disconnect();
					connectInfo = "";	//清空字符
				}
			}
			if( peerType == 1){
				isAsServer = true;
				isAsClient = false;
			}else if( peerType == 2){
				isAsServer = false;
				isAsClient = true;
			}
			GUILayout.EndHorizontal();
			GUILayout.Box(connectInfo, GUILayout.Width(350), GUILayout.Height(60));
			GUILayout.EndArea();
			break;
		case 2:	//自己玩

			if(Network.peerType != NetworkPeerType.Disconnected){
				Network.Disconnect();	//断开网络
			}

			GUI.Box( new Rect(20,80,200,100),"Have Fun!");
			break;
		}
		//退几步棋
		if( toolbarSelectID != 2 ){	//自己玩悔棋次数不限
			GUILayout.BeginArea(new Rect(20,200,200,100));
			GUILayout.Label("可退几步棋: "+canBackSteps);
			canBackSteps = (int)GUILayout.HorizontalSlider(canBackSteps,0,10);
			GUILayout.EndArea();
		}
		//开始按钮

			GUILayout.BeginArea(new Rect(20,250,200,100));
		if(toolbarSelectID != 1){	//网络对战不显示开始按钮
			if(GUILayout.Button("开始游戏")){
				if(toolbarSelectID == 0){	//人机对战
					GameManager.playWithWho = GameManager.PLAY_WITH_AI;
					GameManager.whoGoFirst = whoGoFirst;
					GameManager.hardLevel = level;
					GameManager.canBackSteps = canBackSteps;
					Application.LoadLevel("game");				//切换到游戏场景
				}else if(toolbarSelectID == 1){	//联网对战
//					GameManager.playWithWho = GameManager.PLAY_WITH_NET;
//					if(isAsServer){
//						GameManager.whoGoFirst = GameManager.FIRST_ME;	//服务端先走
//					}else if(isAsClient){
//						GameManager.whoGoFirst = GameManager.FIRST_OTHER;	//如果不是服务端后走
//					}
//					GameManager.canBackSteps = canBackSteps;
//					Application.LoadLevel("game");
				}else if(toolbarSelectID == 2){	//自己玩耍
					GameManager.playWithWho = GameManager.PLAY_WITH_ME;
					GameManager.canBackSteps = 99;
					Application.LoadLevel("game");
				}
			}
		}
		if(GUILayout.Button("退出游戏")){
			Application.Quit();
		}
		GUILayout.EndArea();
		

		//网络连接状态
		switch(Network.peerType){
		case NetworkPeerType.Disconnected:	//未连接
			//connectInfo = "连接断开";
			break;
		case NetworkPeerType.Client:	//作为客户端
			connectInfo = "连接服务器成功";
			break;
		case NetworkPeerType.Server:	//作为服务端
			if( Network.connections.Length <= 0 ){	//没有客户端连接
				if(Time.time - oldTime > 0.3){	//每0.3s
					oldTime = Time.time;
					frame++;
					if(frame > 3){
						frame = 0;
					}
				}
				if(frame == 0){
					connectInfo = "服务端监听中,等待客户端连接";
				}else if(frame == 1){
					connectInfo = "服务端监听中,等待客户端连接.";
				}else if(frame == 2){
					connectInfo = "服务端监听中,等待客户端连接..";
				}else if(frame == 3){
					connectInfo = "服务端监听中,等待客户端连接...";
				}
			}else if(Network.connections.Length > 0){	//有客户端连接
				Debug.Log("We have clients");
				connectInfo = "客户端 : " + Network.connections[0].ipAddress + "["+ Network.connections[0].port+"] 已连接可开始游戏." ;
			}
			break;
		case NetworkPeerType.Connecting:	//尝试连接服务器
			Debug.Log("connecting...");
			connectInfo = "连接中...";
			break;
		}

		if(Network.connections.Length > 0){	//开始游戏
			GameManager.playWithWho = GameManager.PLAY_WITH_NET;
			if(isAsServer){
				GameManager.whoGoFirst = GameManager.FIRST_ME;	//服务端先走
			}else if(isAsClient){
				GameManager.whoGoFirst = GameManager.FIRST_OTHER;	//如果不是服务端后走
			}
			GameManager.canBackSteps = canBackSteps;
			Application.LoadLevel("game");
		}
		if(Time.time - connectTime > 10.0f && connectTime != 0 && Network.peerType != NetworkPeerType.Client){
			connectInfo = "连接服务器失败,请确认IP和端口是否正确,且已连入网络.";
			Network.Disconnect();
			connectTime = 0;
		}
	}
	
	// Update is called once per frame
	void Update () {

	}

	//开启服务器
	void StartServer(){

		int port = 0;	//端口
		try{
			port = int.Parse (serverPort);
		}catch( Exception e ){
			//Debug.Log("Catch you !");
			connectInfo = "请输入合法端口!"+e.Message;
			return;
		}
		Debug.Log ("ServerPort:" + port);
		NetworkConnectionError error = Network.InitializeServer (10, port,false);
		if(error == NetworkConnectionError.NoError){
			connectInfo = "服务端监听中,等待客户端连接...";
		}else{
			connectInfo = "开启失败:"+error;
			return;
		}

	}
	
	//连接服务器
	void ConnectServer(){
		string ip = serverIP;
		int port = 0;
		try{
			port = int.Parse (serverPort);
		}catch( Exception e ){
			connectInfo = "请输入合法端口!"+e.Message;
			return;
		}
		//Debug.Log (ip + ":" + port);
		NetworkConnectionError error = Network.Connect (ip,port);
		Debug.Log (error);
		connectInfo = "连接服务器中...";
		connectTime = Time.time;
	}

//	void GetIPs(){
//		Debug.Log("Unicast Addresses");
//		NetworkInterface[] adapters  = NetworkInterface.GetAllNetworkInterfaces();
//		foreach (NetworkInterface adapter in adapters)
//		{
//			IPInterfaceProperties adapterProperties = adapter.GetIPProperties();
//			UnicastIPAddressInformationCollection uniCast = adapterProperties.UnicastAddresses;
//			if (uniCast.Count >0)
//			{
//				Debug.Log(adapter.Description);
//				foreach (UnicastIPAddressInformation uni in uniCast)
//				{
//					Debug.Log("  Unicast Address ......................... : {0}"+ uni.Address);
//				}
//			}
//		}
//	}
}
