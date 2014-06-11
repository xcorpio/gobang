using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public const int ROW_COUNT = 9;	//行数
	public const int COLUMN_COUNT = 9; //列数

	public const int PLAY_WITH_AI = 0;	//和AI对战
	public const int PLAY_WITH_NET = 1;	//联网对战
	public const int FIRST_ME = 1;		//自己先走
	public const int FIRST_OTHER = 2;	//不是自己先走
	public const int LEVEL_NORMAL = 1;	//普通难度
	public const int LEVEL_HARD = 2;	//困难难度
	public const int LEVEL_EXPERT = 3;	//专家难度

	public static int playWithWho;	//和谁游戏
	public static int whoGoFirst;	//谁先走
	public static int hardLevel;	//难度级别,1 普通 , 2 困难 3, 专家
	public static int canBackSteps;	//可连续退步数

	public Material blackMaterial;	//黑棋材质
	public Material whiteMaterial;	//白棋材质

	//游戏状态
	public const int STATE_BLACK = 0;	//黑棋走
	public const int STATE_WHITE = 1;	//白棋走
	public const int STATE_WIN = 2;		//胜利
	public const int STATE_LOSE = 3;	//输了
	public const int STATE_GAMING = 4;	//游戏中



	private int gameState;			//游戏当前状态
	private int gameSubState;		//游戏子状态
	private char[,] gameData = new char[ROW_COUNT,COLUMN_COUNT];	//存储游戏棋盘信息,'b' 代表黑子，'w' 代表白子
	private char myChess = 'b';		//自己使用的棋子类型


	// Use this for initialization
	void Start () {
		Debug.Log ("PlayWith: "+playWithWho+"  WhoGoFirst: "+whoGoFirst+"  HardLevel: "+hardLevel+" CanBackCount: "+canBackSteps);
		gameState = STATE_GAMING;	//初始状态
		gameSubState = STATE_BLACK;	//黑棋走
	}
	
	// Update is called once per frame
	void Update () {
		switch (gameState) {
		case STATE_GAMING:	//游戏中
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit)){	//检测射线是否碰到物体
				if(Input.GetMouseButtonDown(0)){		//单击左键
					int id = int.Parse(hit.collider.name) - 1;			//点击对象的编号,从1开始的
					int row = id / ROW_COUNT;						//获得对应数据中行列
					int column = id % ROW_COUNT;
					if(gameData[row,column] !='b' && gameData[row,column]!='w'){	//该位置还没有棋子
						if(gameSubState == STATE_BLACK){
							hit.collider.renderer.material = blackMaterial;	//黑棋走赋予黑色材质
							gameSubState = STATE_WHITE;						//改为该白棋走
							gameData[row,column] = 'b';						//存数据							
						}else if(gameSubState == STATE_WHITE){
							hit.collider.renderer.material = whiteMaterial;	//该白棋有赋予白色材质
							gameSubState = STATE_BLACK;						//改为该黑棋走
							gameData[row,column] = 'w';						//存数据
						}
						hit.collider.renderer.enabled = true;				//显示对象
						IsGameOver(row,column);								//判断是否游戏结束
						
						Debug.Log(id + "("+row+","+column+") :"+gameData[row,column] + " GameState: " + gameState);	//调试输出数据
					}else{	//该位置有棋子
						
					}
				}
			}
			break;
		case STATE_WIN:
			break;
		}

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
}
      