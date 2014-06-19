using UnityEngine;
using System.Collections;

public class AI : MonoBehaviour {
	
	static int ROW_COUNT = GameManager.ROW_COUNT;	//行数
	static int COLUMN_COUNT = GameManager.COLUMN_COUNT;	//列数

	public static string[] TYPE_STRINGS = {"单子","死2","活2","死3","活3","死4","活4","成5"};
	//一个方向上的状态
	public const int SINGLE = 0;	//单子
	public const int SI_2 = 1;		//死2
	public const int HUO_2 = 2;		//活2
	public const int SI_3 = 3;		//死3
	public const int HUO_3 = 4;		//活3
	public const int SI_4 = 5;		//死4
	public const int HUO_4 = 6;		//活4
	public const int CHENG_5 = 7;	//成5

	/*获得指定位置'c' 类型棋的得分
	 * chessBoard 当前盘面
	 * c 哪类棋的得分
	 * row,column 位置
	 * level 难度级别
	 * 
	 * 成5:即构成五子连珠, 100分
	 * 活4:即构成两边均不被拦截的四子连珠、双死4、死4活3， 90分
	 * 双活3， 80分
	 * 死3活3， 70分
	 * 死4:一边被拦截的四子连珠， 60分
	 * 活3:两边均不被拦截的三字连珠， 50分
	 * 双活2， 40分
	 * 死3:一边被拦截的三字连珠， 30分
	 * 活2:两边均不被拦截的二子连珠， 20分
	 * 死2:一边被拦截的二子连珠， 10分
	 * 单子:四周无相连棋子, 0分
	 * 
    */
	public static int GetScoreAtPoint(char[,] chessBoard,char c, int row,int column, int level){

		//Debug.Log ("(" + row + "," + column + ")"+c);

		//注意(x,y)和（row,cloumn) 两个坐标系,(x,y)是基于（row,column)的坐标 先这样吧,分析坐标系统和二维数组中的坐标搞混了
		int LEVEL = level;		//难度简单

		int score = 0;		//该位置得分
		int count;		//单个方向棋子数
		int x = column,y = row;		//x,y 水平和垂直位置变量

		int xType;		//x 方向类型,左右
		int xStart;		//左边第一个棋子,
		int xEnd;		//右边最后一个棋子,最与左边一点，可能有空位，实现难度级别
		bool xStartIsOpen;	//xStart左边是否有棋子
		bool xEndIsOpen;		//xEnd右边是否有棋子

		int yType;		//y 方向类型，上下
		int yStart;
		int yEnd;
		bool yStartIsOpen;
		bool yEndIsOpen;

		int xyType;		//左下-右上
		Point xyStart;		//左上角第一个点
		Point xyEnd;		//右下角最后一个点，与左上角第一个点，之间可能有空位
		bool xyStartIsOpen;
		bool xyEndIsOpen;

		int yxType;		//左上-右下
		Point yxStart;
		Point yxEnd;
		bool yxStartIsOpen;
		bool yxEndIsOpen;

		if(LEVEL > 1){	//LEVEL>1 时判定，类型假设判断的点已有棋子:测试时发现LEVEL > 1 有错误之后这样修改
			chessBoard[row,column] = c;
		}
		//x,左右方向
		count = 1;	//本位置,计数1
		xType = SINGLE;
		xStart = xEnd = column;
		xStartIsOpen = xEndIsOpen = false;
		for( x = column-1; x>=0 && column - x <=4 ; --x){//左,最多左5个位置
			if( chessBoard[row,x] == c){//同色
				count++;
				xStart = x;
			}else if((int)chessBoard[row,x] == 0){//没有棋子
				xStartIsOpen = true;
				if(LEVEL == 1){
					break;		//简单：不会跨越空位，目光最短浅
				}
			}else{//异色
				if(chessBoard[row,x+1] == c){	//前一个是自己的棋
					xStartIsOpen = false;
				}
				break;
			}
			if( (x == column - 4 || x == 0) && (int)chessBoard[row,x] != 0 ){//到边缘,且不为空
				xStartIsOpen = false;
			}
		}
		for( x = column+1; x < COLUMN_COUNT && x - column <= 4 ; ++x){//右最多5个位置
			if( chessBoard[row,x] == c){//同色
				count ++;
				xEnd = x;
			}else if( (int)chessBoard[row,x] == 0){//没有棋子
				xEndIsOpen = true;
				if(LEVEL == 1){
					break;		//简单：不会跨越空位，目光最短浅
				}	
			}else{//异色
				if(chessBoard[row,x-1] == c){//前一个是自己的棋
					xEndIsOpen = false;
				}
				break;
			}
			if( (x == column + 4 || x == COLUMN_COUNT -1) && (int)chessBoard[row,x] != 0){	//到边缘,且不为空
				xEndIsOpen = false;
			}
		}
		//Debug.Log ("Level="+LEVEL+"("+row+","+column+")"+c+" xStart:"+xStart+"("+xStartIsOpen+") xEnd:"+xEnd+"("+xEndIsOpen+") ");
		if(LEVEL == 1){//简单
			//count值 此时是指连续的点
			if( count == 1){
				xType = SINGLE;
			}else if( count == 2 ){
				if( (xStartIsOpen && !xEndIsOpen) || (!xStartIsOpen && xEndIsOpen) ){
					//单边开放
					xType = SI_2;
				}else if(xStartIsOpen && xEndIsOpen){
					xType = HUO_2;
				}
			}else if( count == 3){
				if((xStartIsOpen && !xEndIsOpen) || (!xStartIsOpen && xEndIsOpen)){
					xType = SI_3;
				}else if( xStartIsOpen && xEndIsOpen ){
					xType = HUO_3;
				}
			}else if( count == 4){
				if((xStartIsOpen && !xEndIsOpen) || (!xStartIsOpen && xEndIsOpen)){
					xType = SI_4;
				}else if( xStartIsOpen && xEndIsOpen ){
					xType = HUO_4;
				}
			}else if( count >= 5){
				xType = CHENG_5;
			}
		}else if( LEVEL > 1){	//难度大于简单,考虑 x0x中间有空格的情况
			for( int i = xStart; i <= column; ++i){
				count = 0;	//重新计数
				for(int j = i; j <= i+4 && j<= xEnd; ++ j){
					if( chessBoard[row,j] == c ){	//本类qizi
						count ++;
					}
				}
				if(xEnd - i >= 4){//和终点大于4才考虑
					if(chessBoard[row,i] == c ){//同色
						xStartIsOpen = false;
					}else{						//空
						xStartIsOpen = true;
					}
					if(chessBoard[row,i+4] == c ){
						xEndIsOpen = false;
					}else{
						xEndIsOpen = true;
					}
				}
				int temType = SINGLE;	//临时变量，存储当前循环类型
				//count值 此时不一定指连续的点
				if( count == 1){
					temType = SINGLE;
				}else if( count == 2 ){
					if( (xStartIsOpen && !xEndIsOpen) || (!xStartIsOpen && xEndIsOpen) ){
						//单边开放
						temType = SI_2;
					}else if(xStartIsOpen && xEndIsOpen){
						temType = HUO_2;
					}
				}else if( count == 3){
					if((xStartIsOpen && !xEndIsOpen) || (!xStartIsOpen && xEndIsOpen) || (i+4 <= xEnd&&chessBoard[row,i] == c &&chessBoard[row,i+4] == c ) ){//xx00x
						temType = SI_3;
					}else if( xStartIsOpen && xEndIsOpen ){
						temType = HUO_3;
					}
				}else if( count == 4){
					if((xStartIsOpen && !xEndIsOpen) || (!xStartIsOpen && xEndIsOpen) || (i+4 <= xEnd&&chessBoard[row,i] == c &&chessBoard[row,i+4] == c )){//xx0xx
						temType = SI_4;
					}else if( xStartIsOpen && xEndIsOpen ){
						temType = HUO_4;
					}
				}else if( count >= 5){
					temType = CHENG_5;
				}
				if(temType > xType){
					xType = temType;
				}
			}
		}
		//Debug.Log ("xType:"+TYPE_STRINGS [xType]);

		//y,上下方向
		count = 1;	//本位置,计数1
		yType = SINGLE;
		yStart = yEnd = row;
		yStartIsOpen = yEndIsOpen = false;
		for( y = row-1; y>=0 && row - y <=4 ; --y){//上,最多上5个位置
			if( chessBoard[y,column] == c){//同色
				count++;
				yStart = y;
			}else if((int)chessBoard[y,column] == 0){//没有棋子
				yStartIsOpen = true;
				if(LEVEL == 1){
					break;		//简单：不会跨越空位，目光最短浅
				}
			}else{//异色
				if(chessBoard[y+1,column] == c){//上个是自己的棋
					yStartIsOpen = false;
				}
				break;
			}
			if( (y == row - 4 || y == 0) && (int)chessBoard[y,column] != 0){//最上端,且不为空
				yStartIsOpen = false;
			}
		}
		for( y = row+1; y < ROW_COUNT && y - row <= 4 ; ++y){//下最多5个位置
			if( chessBoard[y,column] == c){//同色
				count ++;
				yEnd = y;
			}else if( (int)chessBoard[y,column] == 0){//没有棋子
				yEndIsOpen = true;
				if(LEVEL == 1){
					break;		//简单：不会跨越空位，目光最短浅
				}	
			}else{//异色
				if(chessBoard[y-1,column] == c){//上个是自己的棋
					yEndIsOpen = false;
				}
				break;
			}
			if( (y == row + 4 || y == ROW_COUNT - 1) && (int)chessBoard[y,column] != 0){//边缘且不为空
				yEndIsOpen = false;
			}
		}
		//Debug.Log ("Level="+LEVEL+"("+row+","+column+")"+c+" yStart:"+yStart+"("+yStartIsOpen+") yEnd:"+yEnd+"("+yEndIsOpen+") ");
		if(LEVEL == 1){//简单
			//count值 此时是指连续的点
			if( count == 1){
				yType = SINGLE;
			}else if( count == 2 ){
				if( (yStartIsOpen && !yEndIsOpen) || (!yStartIsOpen && yEndIsOpen) ){
					//单边开放
					yType = SI_2;
				}else if(yStartIsOpen && yEndIsOpen){
					yType = HUO_2;
				}
			}else if( count == 3){
				if((yStartIsOpen && !yEndIsOpen) || (!yStartIsOpen && yEndIsOpen)){
					yType = SI_3;
				}else if( yStartIsOpen && yEndIsOpen ){
					yType = HUO_3;
				}
			}else if( count == 4){
				if((yStartIsOpen && !yEndIsOpen) || (!yStartIsOpen && yEndIsOpen)){
					yType = SI_4;
				}else if( yStartIsOpen && yEndIsOpen ){
					yType = HUO_4;
				}
			}else if( count >= 5){
				yType = CHENG_5;
			}
		}else if( LEVEL > 1){	//难度大于简单
			for( int i = yStart; i <= row; ++i){
				count = 0;	//重新计数
				for(int j = i; j <= i+4 && j<= yEnd; ++ j){
					if( chessBoard[j,column] == c ){	//本类qizi
						count ++;
					}
				}
				if(yEnd - i >= 4){//和终点大于4才考虑
					if(chessBoard[i,column] == c ){
						yStartIsOpen = false;
					}else{
						yStartIsOpen = true;
					}
					if(chessBoard[i+4,column] == c ){
						yEndIsOpen = false;
					}else{
						yEndIsOpen = true;
					}
				}
				int temType = SINGLE;	//临时变量，存储当前循环类型
				//count值 此时不一定指连续的点
				if( count == 1){
					temType = SINGLE;
				}else if( count == 2 ){
					if( (yStartIsOpen && !yEndIsOpen) || (!yStartIsOpen && yEndIsOpen) ){
						//单边开放
						temType = SI_2;
					}else if(yStartIsOpen && yEndIsOpen){
						temType = HUO_2;
					}
				}else if( count == 3){
					if((yStartIsOpen && !yEndIsOpen) || (!yStartIsOpen && yEndIsOpen) || (i+4<=yEnd && chessBoard[i,column] == c &&chessBoard[i+4,column] == c)){//xx00x
						temType = SI_3;
					}else if( yStartIsOpen && yEndIsOpen ){
						temType = HUO_3;
					}
				}else if( count == 4){
					if((yStartIsOpen && !yEndIsOpen) || (!yStartIsOpen && yEndIsOpen) || ( i+4<=yEnd && chessBoard[i,column] == c &&chessBoard[i+4,column] == c)){ //xxoxx
						temType = SI_4;
					}else if( yStartIsOpen && yEndIsOpen ){
						temType = HUO_4;
					}
				}else if( count >= 5){
					temType = CHENG_5;
				}
				if(temType>yType){
					yType = temType;
				}
			}
		}
		//Debug.Log ("yType:"+TYPE_STRINGS[yType]);

		//xy,左上右下
		count = 1;
		xyType = SINGLE;
		xyStart = new Point (column, row);
		xyEnd = new Point (column,row);
		xyStartIsOpen = xyEndIsOpen = false;
		for( x = column - 1,y = row - 1 ;x >= 0 && y>=0 && column - x <= 4&& row - y <=4; --x, --y){//左上
			if( chessBoard[y,x] == c){
				count ++;
				xyStart.x = x;
				xyStart.y = y;
			}else if( (int)chessBoard[y,x] == 0){	//没有棋子
				xyStartIsOpen = true;
				if(LEVEL == 1){
					break;
				}
			}else{	//异色
				if(chessBoard[y+1,x+1] == c){//上个是自己的棋
					xyStartIsOpen = false;
				}
				break;
			}
			if( (x == column - 4 || y == row - 4 || x== 0 || y == 0) && (int)chessBoard[y,x] != 0){//受影响的最端点,且不为空
				xyStartIsOpen =false;
			}
		}
		for( x = column + 1,y = row + 1 ;x < COLUMN_COUNT && y < ROW_COUNT && x - column <= 4&& y - row <=4; ++x, ++y){//右下
			if( chessBoard[y,x] == c){
				count ++;
				xyEnd.x = x;
				xyEnd.y = y;
			}else if( (int)chessBoard[y,x] == 0){	//没有棋子
				xyEndIsOpen = true;
				if(LEVEL == 1){
					break;
				}
			}else{	//异色
				if(chessBoard[y-1,x-1] == c){//上个是自己的棋
					xyEndIsOpen = false;
				}
				break;
			}
			if( (x == column + 4 || y == row + 4 || x == COLUMN_COUNT-1 || y == ROW_COUNT-1) && (int)chessBoard[y,x] != 0){//受影响的最端点
				xyEndIsOpen =false;
			}
		}
		//Debug.Log ("Level="+LEVEL+"("+row+","+column+")"+c+" xyStart:"+xyStart.ToString()+"("+xyStartIsOpen+") xyEnd:"+xyEnd.ToString()+"("+xyEndIsOpen+") ");
		if(LEVEL == 1){//简单
			//count值 此时是指连续的点
			if( count == 1){
				xyType = SINGLE;
			}else if( count == 2 ){
				if( (xyStartIsOpen && !xyEndIsOpen) || (!xyStartIsOpen && xyEndIsOpen) ){
					//单边开放
					xyType = SI_2;
				}else if(xyStartIsOpen && xyEndIsOpen){
					xyType = HUO_2;
				}
			}else if( count == 3){
				//Debug.Log("xyStartIsOpen :"+xyStartIsOpen+"  xyEndIsOpen: "+xyEndIsOpen);
				if((xyStartIsOpen && !xyEndIsOpen) || (!xyStartIsOpen && xyEndIsOpen)){
					xyType = SI_3;
				}else if( xyStartIsOpen && xyEndIsOpen ){
					xyType = HUO_3;
				}
			}else if( count == 4){
				if((xyStartIsOpen && !xyEndIsOpen) || (!xyStartIsOpen && xyEndIsOpen)){
					xyType = SI_4;
				}else if( xyStartIsOpen && xyEndIsOpen ){
					xyType = HUO_4;
				}
			}else if( count >= 5){
				xyType = CHENG_5;
			}
		}else if( LEVEL > 1){	//难度大于简单
			Point i = new Point(xyStart.x,xyStart.y);	//直接赋值，是地址or引用
			for( ; i.x <= column && i.y <= row; ++i.x,++i.y){
				count = 0;	//重新计数
				Point j = new Point(i.x,i.y);
				for(; j.x <= i.x+4 && j.x<= xyEnd.x && j.y <= i.y +4 && j.y <= xyEnd.y ; ++ j.x,++j.y){
					if( chessBoard[j.y,j.x] == c ){	//本类qizi
						count ++;
					}
				}
				if(xyEnd.x - i.x >= 4){//和终点大于4才考虑
					if(chessBoard[i.y,i.x] == c ){
						xyStartIsOpen = false;
					}else{
						xyStartIsOpen = true;
					}
					if(chessBoard[i.y+4,i.x+4] == c ){
						xyEndIsOpen = false;
					}else{
						xyEndIsOpen = true;
					}
				}
				int temType = SINGLE;	//临时变量，存储当前循环类型
				//count值 此时不一定指连续的点
				if( count == 1){
					temType = SINGLE;
				}else if( count == 2 ){
					if( (xyStartIsOpen && !xyEndIsOpen) || (!xyStartIsOpen && xyEndIsOpen) ){
						//单边开放
						temType = SI_2;
					}else if(xyStartIsOpen && xyEndIsOpen){
						temType = HUO_2;
					}
				}else if( count == 3){
					//Debug.Log("xyStartIsOpen :"+xyStartIsOpen+"  xyEndIsOpen: "+xyEndIsOpen);
					if((xyStartIsOpen && !xyEndIsOpen) || (!xyStartIsOpen && xyEndIsOpen) || (i.x +4 <= xyEnd.x && i.y+4 <= xyEnd.y && chessBoard[i.y,i.x] == c && chessBoard[i.y+4,i.x+4] == c)){//x00xx
						temType = SI_3;
					}else if( xyStartIsOpen && xyEndIsOpen ){
						temType = HUO_3;
					}
				}else if( count == 4){
					if((xyStartIsOpen && !xyEndIsOpen) || (!xyStartIsOpen && xyEndIsOpen) || (i.x +4 <= xyEnd.x && i.y+4 <= xyEnd.y && chessBoard[i.y,i.x] == c && chessBoard[i.y+4,i.x+4] == c)){//x0xxx
						temType = SI_4;
					}else if( xyStartIsOpen && xyEndIsOpen ){
						temType = HUO_4;
					}
				}else if( count >= 5){
					temType = CHENG_5;
				}
				if(temType > xyType){
					xyType = temType;
				}
			}
		}
		//Debug.Log ("xyType:"+TYPE_STRINGS[xyType]);

		//yx,左下右上
		count = 1;
		yxType = SINGLE;
		yxStart = new Point (column,row);
		yxEnd = new Point (column,row);
		yxStartIsOpen = yxEndIsOpen = false;
		for( x = column - 1,y = row + 1 ;x >= 0 && y < ROW_COUNT && column - x <= 4&& y - row <=4; --x, ++y){//左下
			if( chessBoard[y,x] == c){
				count ++;
				yxStart.x = x;
				yxStart.y = y;
			}else if( (int)chessBoard[y,x] == 0){	//没有棋子
				yxStartIsOpen = true;
				if(LEVEL == 1){
					break;
				}
			}else{	//异色
				if(chessBoard[y-1,x+1] == c){//上个是自己的棋
					yxStartIsOpen = false;
				}
				break;
			}
			if( (x == column - 4 || y == row + 4 || x== 0 || y == ROW_COUNT-1) && (int)chessBoard[y,x] != 0){//受影响的最端点,且不为空
				yxStartIsOpen =false;
			}
		}
		for( x = column + 1,y = row - 1 ;x < COLUMN_COUNT && y >= 0 && x - column <= 4&& row - y <= 4; ++x, --y){//右上
			if( chessBoard[y,x] == c){
				count ++;
				yxEnd.x = x;
				yxEnd.y = y;
			}else if( (int)chessBoard[y,x] == 0){	//没有棋子
				yxEndIsOpen = true;
				if(LEVEL == 1){
					break;
				}
			}else{	//异色
				if(chessBoard[y+1,x-1] == c){//上个是自己的棋
					yxEndIsOpen = false;
				}
				break;
			}
			if( (x == column + 4 || y == row - 4 || x == COLUMN_COUNT-1 || y == 0) && (int)chessBoard[y,x] != 0){//受影响的最端点,且不为空
				yxEndIsOpen =false;
			}
		}
		//Debug.Log ("Level="+LEVEL+"("+row+","+column+")"+c+" yxStart:"+yxStart.ToString()+"("+yxStartIsOpen+") yxEnd:"+yxEnd.ToString()+"("+yxEndIsOpen+") ");
		if(LEVEL == 1){//简单
			//count值 此时是指连续的点
			if( count == 1){
				yxType = SINGLE;
			}else if( count == 2 ){
				if( (yxStartIsOpen && !yxEndIsOpen) || (!yxStartIsOpen && yxEndIsOpen) ){
					//单边开放
					yxType = SI_2;
				}else if(yxStartIsOpen && yxEndIsOpen){
					yxType = HUO_2;
				}
			}else if( count == 3){
				if((yxStartIsOpen && !yxEndIsOpen) || (!yxStartIsOpen && yxEndIsOpen)){
					yxType = SI_3;
				}else if( yxStartIsOpen && yxEndIsOpen ){
					yxType = HUO_3;
				}
			}else if( count == 4){
				if((yxStartIsOpen && !yxEndIsOpen) || (!yxStartIsOpen && yxEndIsOpen)){
					yxType = SI_4;
				}else if( yxStartIsOpen && yxEndIsOpen ){
					yxType = HUO_4;
				}
			}else if( count >= 5){
				yxType = CHENG_5;
			}
		}else if( LEVEL > 1){	//难度大于简单
			Point i = new Point(yxStart.x,yxStart.y);	//直接赋值，是地址or引用
			for( ; i.x <= column && i.y >= row; ++i.x,--i.y){
				count = 0;	//重新计数
				Point j = new Point(i.x,i.y);
				for(; j.x <= i.x+4 && j.x<= yxEnd.x && j.y >= i.y -4 && j.y >= yxEnd.y ; ++ j.x,--j.y){
					if( chessBoard[j.y,j.x] == c ){	//本类qizi
						count ++;
					}
				}
				if(yxEnd.x - i.x >= 4){//和终点大于4才考虑
					if(chessBoard[i.y,i.x] == c ){
						yxStartIsOpen = false;
					}else{
						yxStartIsOpen = true;
					}
					if(chessBoard[i.y-4,i.x+4] == c ){
						yxEndIsOpen = false;
					}else{
						yxEndIsOpen = true;
					}
				}
				int temType = SINGLE;	//临时变量，存储当前循环类型
				//count值 此时不一定指连续的点
				if( count == 1){
					temType = SINGLE;
				}else if( count == 2 ){
					if( (yxStartIsOpen && !yxEndIsOpen) || (!yxStartIsOpen && yxEndIsOpen) ){
						//单边开放
						temType = SI_2;
					}else if(yxStartIsOpen && yxEndIsOpen){
						temType = HUO_2;
					}
				}else if( count == 3){
					if((yxStartIsOpen && !yxEndIsOpen) || (!yxStartIsOpen && yxEndIsOpen) || (i.x +4 <= yxEnd.x && i.y-4 >= yxEnd.y && chessBoard[i.y,i.x] == c && chessBoard[i.y-4,i.x+4] == c)){//x00xx
						temType = SI_3;
					}else if( yxStartIsOpen && yxEndIsOpen ){
						temType = HUO_3;
					}
				}else if( count == 4){
					if((yxStartIsOpen && !yxEndIsOpen) || (!yxStartIsOpen && yxEndIsOpen) || (i.x +4 <= yxEnd.x && i.y-4 >= yxEnd.y && chessBoard[i.y,i.x] == c && chessBoard[i.y-4,i.x+4] == c)){//x0xxx
						temType = SI_4;
					}else if( yxStartIsOpen && yxEndIsOpen ){
						temType = HUO_4;
					}
				}else if( count >= 5){
					temType = CHENG_5;
				}
				if(temType > yxType){
					yxType = temType;
				}
			}
		}
		//Debug.Log ("yxType:"+TYPE_STRINGS[yxType]);

		if(LEVEL > 1){	//LEVEL>1 时判定，类型假设判断的点已有棋子:测试时发现LEVEL > 1 有错误之后这样修改
			chessBoard[row,column] = '\0';//恢复原样
		}

		//已获得各个方向的类型=======================================
		if( xType == CHENG_5 || yType == CHENG_5 || xyType == CHENG_5 || yxType == CHENG_5){//成5
			score = 100;
		}else if( xType == HUO_4 || yType == HUO_4 || xyType == HUO_4 || yxType == HUO_4
		         || (xType == SI_4 && yType ==SI_4 || xType == SI_4 && xyType == SI_4 || xType == SI_4 && yxType == SI_4 || yType == SI_4 && xyType == SI_4 || yType == SI_4 && yxType ==SI_4 || xyType == SI_4 && yxType == SI_4)
		         || ( xType == SI_4 && yType == HUO_3 || xType == SI_4 && xyType == HUO_3 || xType == SI_4 && yxType == HUO_3 ||
		    		  yType == SI_4 && xType == HUO_3 || yType == SI_4 && xyType == HUO_3 || yType == SI_4 && yxType == HUO_3 ||
		    		  xyType == SI_4 && xType == HUO_3 || xyType == SI_4 && yType == HUO_3 || xyType == SI_4 && yxType == HUO_3 ||
		              yxType == SI_4 && xType == HUO_3 || yxType == SI_4 && yType == HUO_3 || yxType == SI_4 && xyType == HUO_3)){//活4，双死4，死4活3
			score = 90;
		}else if(xType == HUO_3 && yType ==HUO_3 || xType == HUO_3 && xyType == HUO_3 || xType == HUO_3 && yxType == HUO_3 || yType == HUO_3 && xyType == HUO_3 || yType == HUO_3 && yxType ==HUO_3 || xyType == HUO_3 && yxType == HUO_3){//双活三
			score = 80;
		}else if ( xType == SI_3 && yType == HUO_3 || xType == SI_3 && xyType == HUO_3 || xType == SI_3 && yxType == HUO_3
		          || yType == SI_3 && xType == HUO_3 || yType == SI_3 && xyType == HUO_3 || yType == SI_3 && yxType == HUO_3
		          || xyType == SI_3 && xType == HUO_3 || xyType == SI_3 && yType == HUO_3 || xyType == SI_3 && yxType == HUO_3
		          || yxType == SI_3 && xType == HUO_3 || yxType == SI_3 && yType == HUO_3 || yxType == SI_3 && xyType == HUO_3){//死3活3
			score = 70;
		}else if( xType == SI_4 || yType == SI_4 || xyType == SI_4 || yxType == SI_4){//死4
			score = 60;
		}else if ( xType == HUO_3 || yType == HUO_3 || xyType == HUO_3 || yxType == HUO_3){//活3
			score = 50;
		}else if ( xType == HUO_2 && yType == HUO_2 || xType == HUO_2 && xyType == HUO_2 || xType == HUO_2 && yxType == HUO_2
		          || yType == HUO_2 && xyType == HUO_2 || yType == HUO_2 && yxType == HUO_2 || xyType == HUO_2 && yxType == HUO_2){//双活2
			score = 40;
		}else if( xType == SI_3 || yType == SI_3 || xyType == SI_3 || yxType == SI_3){//死三
			score = 30;
		}else if ( xType == HUO_2 || yType == HUO_2 || xyType == HUO_2 || yxType == HUO_2){//活2
			score = 20;
		}else if ( xType == SI_2 || yType == SI_2 || xyType == SI_2 || yxType == SI_2){//死2
			score = 10;
		}else{//单子
			score = 0;
		}
		//Debug.Log ("Level:"+LEVEL+" xType:"+TYPE_STRINGS[xType]+" yType:"+TYPE_STRINGS[yType]+" xyType:"+TYPE_STRINGS[xyType]+" yxType:"+TYPE_STRINGS[yxType]+" score:" + score);
		return score;
	}

	/*
	 * 获得对于 'c' 类型棋，最好的下一步落点
	 * chessBoard : 当前局面
	 * c : 己方棋子类型
	 * cc : 对方棋子类型  converse char 
	 * level 难度级别
	 */
	public static Point GetBestPoint(char[,] chessBoard, char c, char cc,int level){

		Point point = new Point (0, 0);		//存储己方最高分数位置
		Point cPoint = new Point (0, 0);	//存储对方最高分数位置

		int maxScore = 0;					//存储己方最高分数
		int cMaxScore = 0;					//对方最高分数

		int row, column;	//代表位置的行，列
		int i = 0;
		PointScore[] scores = new PointScore[ROW_COUNT * COLUMN_COUNT];		//存储己方每个位置的得分
		PointScore[] cScores = new PointScore[ROW_COUNT * COLUMN_COUNT];	//存储对方每个位置的得分

		for( int j =0 ;j<ROW_COUNT*COLUMN_COUNT;++j){//初始化
			scores[j] = new PointScore();
			cScores[j] = new PointScore();
		}

		//获得己方分数
		//加入随机数，随机方向，防止被套路,有得到的分数相等的情况
		Random.seed = (int)Time.time;
		bool var1 = Random.value > 0.5;
		if(var1){
			row = 0;
		}else{
			row = ROW_COUNT - 1;
		}
		for( ; var1 ? row < ROW_COUNT : row >= 0; ){
			Random.seed = (int)Time.time;
			bool var2 = Random.value > 0.5;
			if(var2){
				column = 0;
			}else{
				column = COLUMN_COUNT - 1;
			}
			for( ; var2 ? column < COLUMN_COUNT : column >= 0; ){
				scores[i].x = row;
				scores[i].y = column;
				if( (int)chessBoard[row,column] == 0){//没有棋子
					scores[i].score = GetScoreAtPoint(chessBoard, c, row, column,level);	//获得该位置得分
					if( scores[i].score > maxScore){
						maxScore = scores[i].score;	//保存当前最高分，及位置
						point.x = row;
						point.y = column;
					}
					++i;
				}else{//有棋子
					scores[i++].score = 0;
				}
				if(var2){
					++column;
				}else{
					--column;
				}
			}
			if(var1){
				++ row;
			}else{
				--row;
			}
		}

		//获得对方分数
		i = 0;
		Random.seed = (int)Time.time;
		var1 = Random.value > 0.5;
		if(var1){
			row = 0;
		}else{
			row = ROW_COUNT - 1;
		}
		for( ; var1 ? row < ROW_COUNT : row >= 0; ){
			Random.seed = (int)Time.time;
			bool var2 = Random.value > 0.5;
			if(var2){
				column = 0;
			}else{
				column = COLUMN_COUNT - 1;
			}
			for( ; var2 ? column < COLUMN_COUNT : column >= 0; ){
				cScores[i].x = row;
				cScores[i].y = column;
				if( (int)chessBoard[row,column] == 0){//没有棋子
					cScores[i].score = GetScoreAtPoint(chessBoard, cc, row, column,level);	//获得该位置得分
					if( cScores[i].score > cMaxScore){
						cMaxScore = cScores[i].score;	//保存当前最高分，及位置
						cPoint.x = row;
						cPoint.y = column;
					}
					++i;
				}else{//有棋子
					cScores[i++].score = 0;
				}
				if(var2){
					++column;
				}else{
					--column;
				}
			}
			if(var1){
				++ row;
			}else{
				--row;
			}
		}
		if(level == 1){
			if( cMaxScore > maxScore){//防御
				point.x = cPoint.x;
				point.y = cPoint.y;
			}
		}else if(level > 1){	//难度>1
			ArrayList maxScoreList = new ArrayList();	//己方得分最高集合
			ArrayList cMaxScoreList = new ArrayList();	//对方得分最高集合
			ArrayList resultList = new ArrayList();		//存放结果点
			for(int m=0 ;m<ROW_COUNT*COLUMN_COUNT;++m){
				if(scores[m].score == maxScore){
					maxScoreList.Add(scores[m]);
				}
				if(cScores[m].score == cMaxScore){
					cMaxScoreList.Add(cScores[m]);
				}
			}
			//Debug.Log("max: "+maxScoreList.Count+"cMax: "+cMaxScoreList.Count);
			if(cMaxScore > maxScore){//用cMaxScoreList 中的点
//				for( int n = 0;n<maxScoreList.Count;++n){//攻防兼备
//					for( int l = 0;l<cMaxScoreList.Count;++l){
//						if(((PointScore)cMaxScoreList[l]).x == ((PointScore)maxScoreList[n]).x && ((PointScore)cMaxScoreList[l]).y == ((PointScore)maxScoreList[n]).y){
//							resultList.Add(maxScoreList[n]);
//							break;
//						}
//					}
//				}
//				if(resultList.Count > 0){
//					point = GetClosePoint(chessBoard,resultList);
//				}else{
					point = GetClosePoint(chessBoard,cMaxScoreList);
//				}
			}else if( cMaxScore <= maxScore ){	//进攻,用maxScoreList中的点
//				for( int n = 0;n<cMaxScoreList.Count;++n){//攻防兼备
//					for( int l = 0;l<maxScoreList.Count;++l){
//						if(((PointScore)maxScoreList[l]).x == ((PointScore)cMaxScoreList[n]).x && ((PointScore)maxScoreList[l]).y == ((PointScore)cMaxScoreList[n]).y){
//							resultList.Add(cMaxScoreList[n]);
//							break;
//						}
//					}
//				}
//				if(resultList.Count > 0){
//					point = GetClosePoint(chessBoard,resultList);
//				}else{
					point = GetClosePoint(chessBoard,maxScoreList);
//				}
			}
		}

		return point;
	}

	//获得里中心最近的点
	public static Point GetClosePoint(char[,] board, ArrayList list){
		float x = 0;
		float y = 0;
		int count = 0;	//计数
		for(int i=0;i<ROW_COUNT;++i){
			for(int j=0;j<COLUMN_COUNT;++j){
				if((int)board[i,j]!=0){
					x += i;
					y += j;
					count ++;
				}
			}
		}
		x = x / count;
		y = y / count;

		float dis = (((PointScore)list[0]).x - x)*(((PointScore)list[0]).x - x) + (((PointScore)list[0]).y - y)*(((PointScore)list[0]).y - y); 	//距离
		Point p = new Point (((PointScore)list[0]).x,((PointScore)list[0]).y);
		float tmp = 0;
		for( int m = 1; m<list.Count; ++m){
			tmp = (((PointScore)list[m]).x - x)*(((PointScore)list[m]).x - x) + (((PointScore)list[m]).y - y)*(((PointScore)list[m]).y - y);
			if(tmp < dis){
				dis = tmp;
				p.x = ((PointScore)list[m]).x;
				p.y = ((PointScore)list[m]).y;
			}
		}

		return p;
	}

	// Use this for initialization
	void Start () {
		//char[,] testBoard = {};
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
