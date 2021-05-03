//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class KDTreeManager : MonoBehaviour
//{
//    [SerializeField]
//    public KDTree tree;

//    [SerializeField]
//    public KDTreeNode RootNode;
//    // Start is called before the first frame update
//    void Start()
//    {
//        tree.RootNode = new KDTreeNode();
//        tree.RootNode.negativeChild = new KDTreeNode();
//        tree.RootNode.negativeChild.count = 5;
//        tree.RootNode.positiveChild = new KDTreeNode();
//        tree.RootNode.positiveChild.count = 9;
//        Debug.Log(tree.RootNode.negativeChild.count);
//        Debug.Log(tree.RootNode.positiveChild.count);
//    }

//    // Update is called once per frame
//    void Update()
//    {
        
//    }
//}

//[Serializable]
//public class KDTree
//{
//    [SerializeField]
//    public KDTreeNode RootNode = null;
//}


//[Serializable]
//public class KDTreeNode
//{
//    [SerializeField]
//    public KDTreeNode negativeChild = null;
//    [SerializeField]
//    public KDTreeNode positiveChild = null;
//    public int count = 9;
//}
