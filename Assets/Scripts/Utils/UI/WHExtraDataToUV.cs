using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace DTT.KVA.UI
{
    [RequireComponent(typeof(Image))]
    public class WHExtraDataToUV : BaseMeshEffect
    {
        /// <summary>
        /// the extra data the ui shader can work with
        /// </summary>
        public Vector2 extraData;

        /// <summary>
        /// protected constructor so it dousnt get created with new
        /// </summary>
        protected WHExtraDataToUV()
        { }

        /// <summary>
        /// modifys the mesh on update of the mesh
        /// </summary>
        /// <param name="vh">the vertexhelper for getting all the verts of a mesh</param>
        public override void ModifyMesh(VertexHelper vh)
        {
            RectTransform rectTransform = (RectTransform)transform;
            Vector2 sizedelta = rectTransform.rect.size;

            UIVertex vert = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                vert.uv1 = sizedelta;
                vert.uv2 = extraData;
                vh.SetUIVertex(vert, i);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// when the object is created makes shure that there is a image attached and also adds the material and sets the height
        /// </summary>
        new protected void Reset()
        {
            Image i = GetComponent<Image>();
            if(i == null)
                i = gameObject.AddComponent<Image>();
            i.material = (Material)AssetDatabase.LoadAssetAtPath("Assets/Shaders/UIRound.mat", typeof(Material));
            i.sprite = null;
            extraData.x = ((RectTransform)transform).rect.height;
        }
#endif
    }
}
