using UnityEngine;
using System.Collections;

/**
 * A) 能够人机对弈，可选择人先走或人后走，具体规则参考通用的五子棋规则；
 * B) 能够悔棋，最多连续悔5步棋；
 * C) 设置游戏的难易程度；
 * D) 可选功能，可联网2人对战。
*/

public class MenuSet : MonoBehaviour {

	string[] toolbarStrings = { "人机对战", "网络对战" ,"自己玩耍"};	//工具条按钮显示字符串
	int toolbarSelectID;		//选择了工具条那个按钮

	bool isMeGoFirst = true;		//自己先走
	bool isAIGoFirst = false;		//AI先走
	int whoGoFirst;					//谁先走, 1 自己 ,2 AI .用Toggle 开关 实现单选功能

	bool isNormalLevel = true;		//普通难度
	bool isHardlevel = false;		//困难难度
	bool isExpertLevel = false;			//专家难度
	int level;						//难度级别, 1 普通 ,2 困难 ,3 专家  同样为实现单选功能

	int canBackSteps = 5;			//连续悔几步棋

	void OnGUI(){
		GUILayout.BeginArea (new Rect (10, 10, 200, 100));
		toolbarSelectID = GUILayout.Toolbar (toolbarSelectID, toolbarStrings);
		GUILayout.EndArea ();

		switch(toolbarSelectID){
		case 0:	//人机对战
			//谁先走
			GUILayout.BeginArea(new Rect(10,100,100,100));
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
			GUILayout.BeginArea(new Rect(120,100,100,150));
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
			break;
		case 2:	//自己玩
			break;
		}
		//退几步棋
		GUILayout.BeginArea(new Rect(10,200,200,100));
		GUILayout.Label("可连续退几步棋: "+canBackSteps);
		canBackSteps = (int)GUILayout.HorizontalSlider(canBackSteps,0,10);
		GUILayout.EndArea();
		//开始按钮
		GUILayout.BeginArea(new Rect(10,250,200,100));
		if(GUILayout.Button("开始游戏")){
			if(toolbarSelectID == 0){	//人机对战
				GameManager.playWithWho = GameManager.PLAY_WITH_AI;
				GameManager.whoGoFirst = whoGoFirst;
				GameManager.hardLevel = level;
				GameManager.canBackSteps = canBackSteps;
				Application.LoadLevel("game");				//切换到游戏场景
			}else if(toolbarSelectID == 1){	//联网对战

			}else if(toolbarSelectID == 2){	//自己玩耍
				GameManager.playWithWho = GameManager.PLAY_WITH_ME;
				GameManager.canBackSteps = canBackSteps;
				Application.LoadLevel("game");
			}
		}
		GUILayout.EndArea();
	}
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
