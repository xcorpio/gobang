using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour {
	
	public static bool isMusicOn = true;			//是否播放声音,其他文件也需访问
	public AudioSource music;		//背景音乐

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 ,MouseScrollWheel = 3}
	public RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 5F;		//顺时针旋转灵敏度
	public float sensitivityY = 5F;		//前后旋转灵敏度
	public float sensitivityScale = 5F;		//缩放灵敏度
	public float sensitivityMove = 5F;		//移动灵敏度

	private Vector3 qiPanOriginalPosition;	//棋盘原始位置
	private Quaternion qiPanOriginalRotate;	//原始方向角

	private Vector3 cameraOriginalPosition;	//照相机原始位置

	void Update ()
	{
		if (axes == RotationAxes.MouseXAndY && Input.GetMouseButton(1))		//按住鼠标右键旋转
		{
			transform.Rotate(-Input.GetAxis("Mouse Y") * sensitivityY, Input.GetAxis("Mouse X") * sensitivityX, 0,Space.World);	//上下左右一起旋转
		}
		else if (axes == RotationAxes.MouseX && Input.GetMouseButton(1))
		{
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0, Space.World);	//左右旋转
		}
		else if( axes == RotationAxes.MouseY && Input.GetMouseButton(1))
		{
			transform.Rotate(-Input.GetAxis("Mouse Y") * sensitivityY, 0, 0, Space.World);	//上下旋转
		}
		//else if( axes == RotationAxes.MouseScrollWheel ){	//滚轮
			float wheel = Input.GetAxis ("Mouse ScrollWheel");
			if(wheel != 0){
				//transform.localScale += new Vector3(wheel,0,wheel); 	//代价太大
				Vector3 position = Camera.main.transform.position;
				position.y += wheel * sensitivityScale;					//移动摄像机位置实现放大缩小
				Camera.main.transform.position = position;
			}
		//}
		//   鼠标中键移动物体
		if(Input.GetMouseButton(2)){
			transform.Translate (Input.GetAxis ("Mouse X") * sensitivityMove, 0, -Input.GetAxis ("Mouse Y") * sensitivityMove,Space.World);
		}
	}

	//棋盘复位
	void PositionReset(){
		transform.position = qiPanOriginalPosition;
		Camera.main.transform.position = cameraOriginalPosition;
		transform.rotation = qiPanOriginalRotate;
	}

	void OnGUI(){
		GUILayout.BeginArea (new Rect (Screen.width - 100, 10, 80, 100));
		isMusicOn = GUILayout.Toggle (isMusicOn, "<color=yellow>声音开关</color>");
		if(isMusicOn){
			if(!music.isPlaying  && music != null){
				music.Play();
			}
		}else{
			if(music.isPlaying  && music != null){
				music.Pause();				//关闭声音
			}
		}
		if(GUILayout.Button("复位")){
			PositionReset();
		}
		GUILayout.EndArea ();
	}
	void Start ()
	{
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
		//保存初始位置，方向
		qiPanOriginalPosition = transform.position;
		qiPanOriginalRotate = transform.rotation;
		cameraOriginalPosition = Camera.main.transform.position;

		if(music != null){
			music.Play();
		}
	}
}
