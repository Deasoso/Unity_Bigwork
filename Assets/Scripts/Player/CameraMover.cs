﻿	using UnityEngine;  
　　using System.Collections;  
　　  
　　public class CameraMover : MonoBehaviour {  

	 
　　  public Transform RealityPlayer;  
　　  public float smoothRate = 0.5f;
		public float distance = 30;
		public static bool guding;
	  private Transform Player;
　　  private Transform thisTransform;  
　　  private Vector2 velocity;  
　　  
　　  // Use this for initialization  
　　  void Start () {  
		Player = RealityPlayer;
　　      thisTransform = transform;  
　　      velocity = new Vector2 (0.5f, 0.5f);  
　　  }  
　　    
　　  // Update is called once per frame  
　　  void Update () {  
		Player = RealityPlayer;
		if (guding){
			transform.Translate(new Vector3(0,0,-10));
			return;
		 }
　　      Vector2 newPos2D = Vector2.zero;  
　　      //Mathf.SmoothDamp平滑阻尼，这个函数用于描述随着时间的推移逐渐改变一个值到期望值，这里用于随着时间的推移（0.5秒）让摄像机跟着角色的移动而移动  
　　      newPos2D.x = Mathf.SmoothDamp (thisTransform.position.x, Player.position.x, ref velocity.x, smoothRate);  
　　      newPos2D.y = Mathf.SmoothDamp (thisTransform.position.y, Player.position.y, ref velocity.y, smoothRate);  
　　    
　　      Vector3 newPos = new Vector3 (newPos2D.x, newPos2D.y, -1 * distance);  
　　      //Vector3.Slerp 球形插值，通过t数值在from和to之间插值。返回的向量的长度将被插值到from到to的长度之间。time.time此帧开始的时间（只读）。这是以秒计算到游戏开始的时间。也就是说，从游戏开始到到现在所用的时间。  
　　      transform.position = Vector3.Slerp (transform.position, newPos, Time.time);  
　　  }  
	void rotateandyuan(){
		
		//Debug.Log(transform.position.x+","+transform.position.y+","+Time.deltaTime*-5);
	}
　}  