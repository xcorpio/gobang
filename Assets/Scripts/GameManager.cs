using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public const int ROW_COUNT = 15;	//行数
	public const int COLUMN_COUNT = 15; //列数
	
	//声音对象
	public AudioSource musicPutStone;	//落子声音
	public AudioSource musicError;		//错误提示声音
	public AudioSource musicUndo;		//悔棋声音

	//贴图
	public Texture texLose;		//失败贴图
	public Texture texWin;		//胜利贴图
	private bool isShowOverWindow = false;	//是否显示结束窗口
	private bool isHasShowOverWindow = false;	//是否已经显示过
	private Rect windowRectWin = new Rect((Screen.width - 350)/2,(Screen.height - 250)/2,350,250);	//提示胜利失败窗口位置
	private Rect windowRectLose = new Rect((Screen.width - 350)/2,(Screen.height - 250)/2,350,250);

	public const int PLAY_WITH_AI = 1;	//和AI对战
	public const int PLAY_WITH_NET = 2;	//联网对战
	public const int PLAY_WITH_ME = 3;	//自己单机玩

	public const int FIRST_ME = 1;		//自己先走
	public const int FIRST_OTHER = 2;	//不是自己先走

	public const int LEVEL_NORMAL = 1;	//普通难度
	public const int LEVEL_HARD = 2;	//困难难度
	public const int LEVEL_EXPERT = 3;	//专家难度

	public static int playWithWho;	//和谁游戏
	public static int whoGoFirst;	//谁先走
	public static int hardLevel;	//难度级别,1 普通 , 2 困难 3, 专家
	public static int canBackSteps;	//可连续退步数
	public static int maxBackSteps;	//最多可以回退数

	public Material blackMaterial;	//黑棋材质
	public Material blackRedMaterial;
	public Material whiteMaterial;	//白棋材质
	public Material whiteRedMaterial;

	//游戏状态
	public const int STATE_BLACK = 1;	//黑棋走
	public const int STATE_WHITE = 2;	//白棋走
	public const int STATE_WIN = 3;		//胜利
	public const int STATE_LOSE = 4;	//输了
	public const int STATE_GAMING = 5;	//游戏中

	
	private int gameState;			//游戏当前状态
	private int gameSubState;		//游戏子状态
	private char[,] gameData = new char[ROW_COUNT,COLUMN_COUNT];	//存储游戏棋盘信息,'b' 代表黑子，'w' 代表白子
	private char myChess;		//指定自己使用的棋子类型，在 IsGameOver() 中判断输赢
	private bool isMyTurn;		//是否该我下
	private Stack record = new Stack ();	//记录每步的栈,用于悔棋
	private bool isBacking ;	//指示是否是自己在悔棋，只用在网络对战中

	// Use this for initialization
	void Start () {
		Debug.Log ("PlayWith: "+playWithWho+"  WhoGoFirst: "+whoGoFirst+"  HardLevel: "+hardLevel+" CanBackCount: "+canBackSteps);
		gameState = STATE_GAMING;	//初始状态
		maxBackSteps = canBackSteps;	//
		if( whoGoFirst == FIRST_ME && playWithWho == PLAY_WITH_AI || playWithWho == PLAY_WITH_ME){//我先走
			gameSubState = STATE_BLACK;	//黑棋走
			myChess = 'b';
			isMyTurn = true;
		}else if( whoGoFirst == FIRST_OTHER && playWithWho == PLAY_WITH_AI){	//AI先走
			gameData[ROW_COUNT/2,COLUMN_COUNT/2] = 'b';
			if(MouseLook.isMusicOn){
				if(musicPutStone != null){
					musicPutStone.Play();
				}
			}
			StartCoroutine(PutChessAtPoint(new Point(ROW_COUNT / 2, COLUMN_COUNT / 2),'b'));	//C# 必须自己调用StartCoroutine
			gameSubState = STATE_WHITE;	//白棋走
			myChess = 'w';
			isMyTurn = true;
			record.Push( new Point(ROW_COUNT/2, COLUMN_COUNT/2));			//记录压栈
			LightLastChess();
		}
		if( whoGoFirst == FIRST_ME && playWithWho == PLAY_WITH_NET ){	//net对战，我先走，此处代码冗余，暂不考虑
			gameSubState = STATE_BLACK;	//黑棋走
			myChess = 'b';
			isMyTurn = true;
		}else if(whoGoFirst == FIRST_OTHER && playWithWho == PLAY_WITH_NET){	//net对战，对方先走
			gameSubState = STATE_BLACK;	//黑棋走
			myChess = 'w';
			isMyTurn = false;
		}

	}
	
	// Update is called once per frame
	void Update () {
		switch (gameState) {
		case STATE_GAMING:	//游戏中
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)){	//检测射线是否碰到物体
				if(Input.GetMouseButtonUp(0)){		//单击左键弹起该响应位置
					int id = int.Parse(hit.collider.name) - 1;			//点击对象的编号,从1开始的
					int row = id / ROW_COUNT;						//获得对应数据中行列
					int column = id % ROW_COUNT;
					if((int)gameData[row,column] == 0){	//该位置还没有棋子
						if(isMyTurn && gameSubState == STATE_BLACK ){
							hit.collider.renderer.material = blackMaterial;	//黑棋走赋予黑色材质
							gameSubState = STATE_WHITE;						//改为该白棋走
							if(Network.peerType != NetworkPeerType.Disconnected && playWithWho == PLAY_WITH_NET){//发给网友
								networkView.RPC("NetPutChess",RPCMode.Others,row,column);
							}
							//AI.GetScoresMap(gameData,'b');
							gameData[row,column] = 'b';						//存数据	
							if(MouseLook.isMusicOn){		//播放声音
								if(musicPutStone != null){
									musicPutStone.Play();
								}
							}
							if( playWithWho == PLAY_WITH_ME ){
								isMyTurn = true;
							}else{
								isMyTurn = false;								//轮到其他人下
							}
//							if(canBackSteps < maxBackSteps){	//可连续悔五步棋，不是可以悔5次，注释这个分支就可以 总共悔5次
//								canBackSteps ++;				//没到 5 之前 ++
//							}
							hit.collider.renderer.enabled = true;				//显示对象
							record.Push( new Point(row,column));				//记录压栈
							LightLastChess();
							IsGameOver(row,column);								//判断是否游戏结束
							//Debug.Log("score:"+AI.GetScoreAtPoint(gameData,'b',row,column)) ;
							//AI.GetScoresMap(gameData,'b');
						}else if(isMyTurn && gameSubState == STATE_WHITE ){	//鼠标控制点击
							if( playWithWho == PLAY_WITH_AI || playWithWho == PLAY_WITH_NET){
								isMyTurn = false;
//								if(canBackSteps < maxBackSteps){	//可连续悔五步棋，不是可以悔5次，注释这个分支就可以 总共悔5次
//									canBackSteps ++;				//没到 5 之前 ++
//								}
							}else if( playWithWho == PLAY_WITH_ME ){//和自己玩
								isMyTurn = true;
							}
							if(Network.peerType != NetworkPeerType.Disconnected && playWithWho == PLAY_WITH_NET){//发给网友
								networkView.RPC("NetPutChess",RPCMode.Others,row,column);
							}
							hit.collider.renderer.material = whiteMaterial;	//该白棋有赋予白色材质
							gameSubState = STATE_BLACK;						//改为该黑棋走
							gameData[row,column] = 'w';						//存数据
							if(MouseLook.isMusicOn){		//播放声音
								if(musicPutStone != null){
									musicPutStone.Play();
								}
							}
							hit.collider.renderer.enabled = true;				//显示对象
							record.Push( new Point(row,column));				//记录压栈
							LightLastChess();
							IsGameOver(row,column);								//判断是否游戏结束
						}
					}else{	//该位置已经有棋子
						
					}
				}
			}
			// AI ,其他人下子
			if( !isMyTurn && playWithWho != PLAY_WITH_ME && gameState == STATE_GAMING){//IsGameOver()运行后gameState 可能已经改变
				if( gameSubState == STATE_WHITE ){	//其他人该下白子
					if( playWithWho == PLAY_WITH_AI){				//和AI玩
						Point p = AI.GetBestPoint(gameData, 'w', 'b',hardLevel);
						Debug.Log("AI's next position :("+ p.x + ","+ p.y+")");
						gameSubState = STATE_BLACK;						//改为该黑棋走
						gameData[p.x,p.y] = 'w';						//存数据
						StartCoroutine(PutChessAtPoint( p,'w'));				//C# 脚本这样写
						isMyTurn = true;			//该我下了
						record.Push(p);				//记录压栈
						LightLastChess();
						IsGameOver(p.x,p.y);
					}else if ( playWithWho == PLAY_WITH_NET ){	//联网对战,网友下白子
						//Debug.Log ("net white");	//由NetPutChess 控制
					}
				}else if (gameSubState == STATE_BLACK ){	//其他人该下黑子
					if( playWithWho == PLAY_WITH_AI ){	//AI下黑子
						Point p = AI.GetBestPoint(gameData, 'b', 'w',hardLevel);
						Debug.Log("AI's next position :("+ p.x + ","+ p.y+")");
						gameSubState = STATE_WHITE;						//改为该白棋走
						gameData[p.x,p.y] = 'b';						//存数据
						StartCoroutine(PutChessAtPoint( p,'b'));				//C# 脚本这样写
						isMyTurn = true;			//该我下了
						record.Push(p);				//记录压栈
						LightLastChess();
						IsGameOver(p.x,p.y);
					}else if( playWithWho == PLAY_WITH_NET ){	//网友下黑子
						//Debug.Log ("net black");
					}
				}
			}
			break;
		case STATE_WIN:
			isShowOverWindow = true;
			break;
		case STATE_LOSE:
			isShowOverWindow = true;
			break;
		}
	}

	//高亮上一次下的棋子
	void LightLastChess(){
		if(record.Count > 0){	//改变上一次棋的材质
			Point p = (Point)record.Peek();
			int id;		//棋子ID
			id = p.x * ROW_COUNT + p.y + 1;	//棋子编号从1开始
			GameObject chess = GameObject.Find ("qi_pan1/"+id);
			if( gameData[p.x,p.y] == 'b' ){
				chess.renderer.material = blackRedMaterial;
			}else if( gameData[p.x,p.y] == 'w' ){
				chess.renderer.material = whiteRedMaterial;
			}
			if(record.Count > 1){//恢复上上个材质
				record.Pop();
				Point p2 = (Point)record.Peek();
				id = p2.x * ROW_COUNT + p2.y + 1;	//棋子编号从1开始
				chess = GameObject.Find ("qi_pan1/"+id);
				if( gameData[p2.x,p2.y] == 'b' ){
					chess.renderer.material = blackMaterial;
				}else if( gameData[p2.x,p2.y] == 'w' ){
					chess.renderer.material = whiteMaterial;
				}
				record.Push(p);
			}
		}
	}

	//接受网友下子信息
	[RPC]
	void NetPutChess(int row, int column){
		Point p = new Point(row,column);
		if( whoGoFirst == FIRST_OTHER ){//该白棋下
			gameSubState = STATE_WHITE;						//改为该白棋走
			gameData[row,column] = 'b';						//存数据
			if(MouseLook.isMusicOn){		//播放声音
				if(musicPutStone != null){
					musicPutStone.Play();
				}
			}
			StartCoroutine(PutChessAtPoint( p,'b'));		//C# 脚本这样写
			isMyTurn = true;			//该我下了
			record.Push(p);				//记录压栈
			LightLastChess();
			IsGameOver(p.x,p.y);
		}else if(whoGoFirst == FIRST_ME ){//该黑棋下
			gameSubState = STATE_BLACK;						//改为该黑棋走
			gameData[p.x,p.y] = 'w';						//存数据
			if(MouseLook.isMusicOn){		//播放声音
				if(musicPutStone != null){
					musicPutStone.Play();
				}
			}
			StartCoroutine(PutChessAtPoint( p,'w'));		//C# 脚本这样写
			isMyTurn = true;			//该我下了
			record.Push(p);				//记录压栈
			LightLastChess();
			IsGameOver(p.x,p.y);
		}
//		if(canBackSteps < maxBackSteps){	//可连续悔五步棋，不是可以悔5次，注释这个分支就可以 总共悔5次
//			canBackSteps ++;				//没到 5 之前 ++
//		}
	}

	//接受net悔棋
	[RPC]
	void NetRollback(){
		Point last = (Point) record.Pop();	//上次下棋位置
		if( gameData[last.x,last.y] == myChess && isBacking || gameData[last.x,last.y] != myChess && !isBacking){	//上次是自己下的,且我在悔棋 或 对方在悔棋
			if(gameData[last.x,last.y] == 'b'){	//当前该位置是黑棋
				gameSubState = STATE_BLACK;		//该黑棋下
			}else{		//白棋
				gameSubState = STATE_WHITE;		//该白棋下
			}
			gameData[last.x,last.y] = '\0';	//清空该位置
			int id = last.x * ROW_COUNT+last.y + 1;	//获得棋子ID从1开始编号
			GameObject chess = GameObject.Find("qi_pan1/"+ id );	//获得被取消的对象
			chess.renderer.enabled = false;	//隐藏该对象
		}else if( gameData[last.x,last.y] != myChess && isBacking || gameData[last.x,last.y] == myChess && !isBacking){	//上一步不是我下的,回退对方的上一步，和自己的上一步 或 对方在悔棋
			//销毁其他上一步
			gameData[last.x,last.y] = '\0';	//清空该位置
			int id = last.x * ROW_COUNT+last.y + 1;	//获得棋子ID从1开始编号
			GameObject chess = GameObject.Find("qi_pan1/"+ id );	//获得被取消的对象
			chess.renderer.enabled = false;	//隐藏该对象
			//销毁自己上一步
			last = (Point) record.Pop();
			if(gameData[last.x,last.y] == 'b'){	//当前该位置是黑棋
				gameSubState = STATE_BLACK;		//该黑棋下
			}else{		//白棋
				gameSubState = STATE_WHITE;		//该白棋下
			}
			gameData[last.x,last.y] = '\0';	//清空该位置
			id = last.x * ROW_COUNT+last.y + 1;	//获得棋子ID从1开始编号
			chess = GameObject.Find("qi_pan1/"+ id );	//获得被取消的对象
			chess.renderer.enabled = false;	//隐藏该对象
		}
		if(isBacking){//是否是自己在悔棋
			isMyTurn = true;
			isBacking = false;
		}else{
			isMyTurn =false;
		}
		if(gameState != STATE_GAMING){	//如果游戏结束，还可以反悔
			gameState = STATE_GAMING;
			isHasShowOverWindow = false;	//使重新可以显示窗口
		}
		LightLastChess ();	//高亮上个棋子
	}

	//悔棋
	void Rollback(){
		if( record.Count <= 0 ){	//栈内没有记录直接返回
			return;
		}
		if( playWithWho == PLAY_WITH_AI && whoGoFirst == FIRST_OTHER && record.Count <= 2){	//如果AI先走，栈中至少有两步，防止用户一开始就后悔
			return;
		}
		if(playWithWho == PLAY_WITH_ME){	//和自己玩，每次只退上一步，不管黑白,不对反悔次数做限制
			Point last = (Point)record.Pop();		//出栈
			if(gameData[last.x,last.y] == 'b'){	//当前该位置是黑棋
				gameSubState = STATE_BLACK;		//该黑棋下
			}else{		//白棋
				gameSubState = STATE_WHITE;		//该白棋下
			}
			gameData[last.x,last.y] = '\0';	//清空该位置
			int id = last.x * ROW_COUNT+last.y + 1;	//获得棋子ID从1开始编号
			GameObject chess = GameObject.Find("qi_pan1/"+ id );	//获得被取消的对象
			chess.renderer.enabled = false;	//隐藏该对象
			if(gameState != STATE_GAMING){	//如果游戏结束，还可以反悔
				gameState = STATE_GAMING;
				isHasShowOverWindow = false;
			}
		}else if( playWithWho == PLAY_WITH_AI){	//AI,如果上一步是自己下的，退一步；是AI下的退两步,经验证没有第一种情况
			if ( GameManager.canBackSteps > 0){	//机会大于0，才可以后退
				Point last = (Point) record.Pop();	//上次下棋位置
				if( gameData[last.x,last.y] == myChess ){	//上次是自己下的, 不可能的啊，AI早下完了,此分支无效
					isMyTurn = true;	//还该我下
					if(gameData[last.x,last.y] == 'b'){	//当前该位置是黑棋
						gameSubState = STATE_BLACK;		//该黑棋下
					}else{		//白棋
						gameSubState = STATE_WHITE;		//该白棋下
					}
					gameData[last.x,last.y] = '\0';	//清空该位置
					int id = last.x * ROW_COUNT+last.y + 1;	//获得棋子ID从1开始编号
					GameObject chess = GameObject.Find("qi_pan1/"+ id );	//获得被取消的对象
					chess.renderer.enabled = false;	//隐藏该对象
				}else if( gameData[last.x,last.y] != myChess ){	//上一步不是我下的,回退AI的上一步，和自己的上一步
					isMyTurn = true;
					//销毁AI上一步
					gameData[last.x,last.y] = '\0';	//清空该位置
					int id = last.x * ROW_COUNT+last.y + 1;	//获得棋子ID从1开始编号
					GameObject chess = GameObject.Find("qi_pan1/"+ id );	//获得被取消的对象
					chess.renderer.enabled = false;	//隐藏该对象
					//销毁自己上一步
					last = (Point) record.Pop();
					if(gameData[last.x,last.y] == 'b'){	//当前该位置是黑棋
						gameSubState = STATE_BLACK;		//该黑棋下
					}else{		//白棋
						gameSubState = STATE_WHITE;		//该白棋下
					}
					gameData[last.x,last.y] = '\0';	//清空该位置
					id = last.x * ROW_COUNT+last.y + 1;	//获得棋子ID从1开始编号
					chess = GameObject.Find("qi_pan1/"+ id );	//获得被取消的对象
					chess.renderer.enabled = false;	//隐藏该对象
				}
				if(gameState != STATE_GAMING){	//如果游戏结束，还可以反悔
					gameState = STATE_GAMING;
					isHasShowOverWindow = false;
				}
				canBackSteps --;
			}else{	//没有机会悔棋
				if(MouseLook.isMusicOn){		//播放声音
					if(musicError != null){
						musicError.Play();
					}
				}
				Debug.Log("悔棋次数已经用完");
			}
		}else if( playWithWho == PLAY_WITH_NET ){	//网络对战
			if(canBackSteps > 0){
				Point p = (Point) record.Peek();
				char c = gameData[p.x,p.y];
				if ( c == myChess && record.Count >= 1 || c != myChess && record.Count >= 2){	//自己下的上一步，至少有一个棋子，对面下的上一步，至少有两个棋子
					isBacking = true;		//设置悔棋标志
					networkView.RPC("ReceiveMessage",RPCMode.Others,"<color=red><i><b>我要悔棋了！</b></i></color>");
					networkView.RPC("NetRollback",RPCMode.All);		//自己和网友都通过这种方式
					canBackSteps -- ;
				}
			}else{
				if(MouseLook.isMusicOn){		//播放声音
					if(musicError != null){
						musicError.Play();
					}
				}
				Debug.Log("悔棋次数已经用完");
			}
		}
		LightLastChess ();	//高亮上个棋子
		//Debug.Log ("Stack Current Size: " + record.Count);
	}

	//在指定位置，显示 ‘c' 代表类型的棋子
	IEnumerator  PutChessAtPoint( Point p ,char c){
		int id;		//棋子ID
		id = p.x * ROW_COUNT + p.y + 1;	//棋子编号从1开始
		GameObject chess = GameObject.Find ("qi_pan1/"+id);
		//Debug.Log ("show chess :" + id +"name:"+ (chess == null ? "null" : chess.name));
		if( c == 'w' ){	//白色
			chess.renderer.material = whiteMaterial;
		}else if ( c == 'b' ){	//黑色
			chess.renderer.material = blackMaterial;
		}
		yield return new WaitForSeconds (0.5f);	//延迟0.5s
		chess.renderer.enabled = true;	//显示
	}

	//检测落下一个棋子时游戏是否结束
	void IsGameOver(int row, int column){
		int count = 1;		//记录几个棋子连在一起了
		int i = row;
		int j = column;
		char c = gameData [row, column];	//棋子类型

		//横向计数
		for ( i = row -1; i >= 0 && gameData[i,column] == c ; --i)	//左
			count ++;
		for (i = row+1; i < COLUMN_COUNT && gameData[i,column] == c ; ++i)	//右
			count ++;
		if (count >= 5) {
			if (c == myChess) {
				gameState = STATE_WIN;
			} else {
				gameState = STATE_LOSE;
			}
			return;
		} else {
			count = 1;
		}
		//垂直方向
		for (j = column - 1; j >= 0 && gameData[row,j] == c; --j)	//上
			count ++;
		for (j= column + 1; j<ROW_COUNT && gameData[row,j] == c; ++j) //下
			count++;
		if (count >= 5) {
			if (c == myChess) {
				gameState = STATE_WIN;
			} else {
				gameState = STATE_LOSE;
			}
			return;
		} else {
			count = 1;
		}
		//斜方向 左上-右下
		for (i =row -1, j = column -1; i>=0 && j>=0 &&gameData[i,j] == c; --i, --j)	//左上
			count ++;
		for (i = row + 1, j =column + 1; i<ROW_COUNT && j < COLUMN_COUNT && gameData[i,j] == c; ++i, ++j) //右下
			count ++;
		if (count >= 5) {
			if (c == myChess) {
				gameState = STATE_WIN;
			} else {
				gameState = STATE_LOSE;
			}
			return;
		} else {
			count = 1;
		}
		//斜方向 左下-右上
		for (i =row - 1, j = column +1; i>=0 && j < COLUMN_COUNT &&gameData[i,j] == c; --i, ++j)	//左下
			count ++;
		for (i = row + 1, j =column - 1; i < ROW_COUNT && j >= 0 && gameData[i,j] == c; ++i, --j)	//右上
			count ++;
		if (count >= 5) {
			if (c == myChess) {
				gameState = STATE_WIN;
			} else {
				gameState = STATE_LOSE;
			}
			return;
		} else {
			count = 1;
		}
	}

	void OnGUI(){
		if( isShowOverWindow && !isHasShowOverWindow){
			if(gameState == STATE_WIN ){	//胜利窗口
				windowRectWin = GUILayout.Window(1,windowRectWin,DoWindow,"You Win");
			}else if(gameState == STATE_LOSE){	//失败窗口
				windowRectLose = GUILayout.Window(2,windowRectLose,DoWindow,"You Lose");
			}
		}
	}

	//绘制窗口
	void DoWindow(int windowID){
		if(windowID == 1){
			GUILayout.Box(texWin);
			if(GUILayout.Button("确定")){
				isHasShowOverWindow = true;
			}
		}else if(windowID == 2){
			GUILayout.Box(texLose);
			if(GUILayout.Button("确定")){
				isHasShowOverWindow = true;
			}
		}
		GUI.DragWindow ();
	}
}
      