using UnityEngine;
using UnityEngine.UI;
public class LoadingUI : MonoBehaviour
{

    [SerializeField]
    Image progressValue;

    [SerializeField]
    Text progressText;

    [SerializeField]
    GameObject progressBar;

    [SerializeField]
    Text progressDesc;

    float m_Max;

    /// <summary>
    /// 初始化进度条
    /// </summary>
    /// <param name="max"></param>
    /// <param name="desc"></param>
    public void InitProgress(float max, string desc)
    {
        m_Max = max;
        progressBar.SetActive(true);
        progressDesc.gameObject.SetActive(true);
        progressDesc.text = desc;
        progressValue.fillAmount = max > 0 ? 0 : 100;
        progressText.gameObject.SetActive(max > 0);
    }

    /// <summary>
    /// 更新进度条
    /// </summary>
    /// <param name="progress"></param>
    public void UpdateProgress(float progress)
    {
        progressValue.fillAmount = progress / m_Max;
        progressText.text = string.Format("{0:0}%", progressValue.fillAmount * 100);
    }

}