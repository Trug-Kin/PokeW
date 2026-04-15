using UnityEngine;

[CreateAssetMenu(fileName = "New Gourdball", menuName = "Items/Gourdball")]
public class GourdballItem : ItemBase
{
    [Tooltip("Hệ số bắt: Gourdball = 1, Great Ball = 1.5, Ultra Ball = 2")]
    public float catchRateModifier = 1f;

    // Gourdball không được dùng trực tiếp lên Pokemon trong menu, mà dùng trong trận chiến
    public override bool Use(Pokemon pokemon)
    {
        // Logic bắt Pokemon (tính tỉ lệ bắt, ném bình...)
        // Trả về true để tiêu tốn vật phẩm sau khi ném
        return true;
    }
}