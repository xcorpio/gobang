using UnityEngine;
using System.Collections;

public class Welcome : MonoBehaviour {
	
	//帧动画数组
	Object[] images;
	int curFrame = 0; 		//当前帧索引
	Texture2D curTexture;	//当前帧贴图
	//音乐文件
	public AudioSource music;

	// Use this for initialization
	void Start () {
		Screen.showCursor = false;	//不显示光标
		images = Resources.LoadAll ("Images/Welcome");
	}
	
	// Update is called once per frame
	void Update () {

	}

	void OnGUI(){
		GUI.DrawTexture(new Rect (0, 0, Screen.width, Screen.height), curTexture, ScaleMode.StretchToFill);
	}

	void FixedUpdate(){
		curFrame ++;
		if (curFrame == 5) {
			music.Play ();					//播放声音
		}
		if(curFrame > images.Length - 1){
			curFrame --;
			Application.LoadLevel("menu");
		}
		curTexture = (Texture2D) images [curFrame];
	}
}
