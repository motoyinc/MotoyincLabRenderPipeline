using UnityEngine;

[DisallowMultipleComponent] //限定一个GameObject只能添加一个脚本
public class PerObjectMaterialProperties : MonoBehaviour {
        
    static int baseColorId = Shader.PropertyToID("_BaseColor");
        
    [SerializeField]
    Color baseColor = Color.white;
    static MaterialPropertyBlock block;

    // 在编辑器状态时属性发生变化，这个函数才会触发
    [ExecuteInEditMode]
    void OnValidate () {
        if (block == null) {
            block = new MaterialPropertyBlock();
        }
                
        //注意先setBlock，再传给Component
        block.SetColor(baseColorId, baseColor);
        GetComponent<Renderer>().SetPropertyBlock(block, 0); // 修改第一个材质球
    }

}