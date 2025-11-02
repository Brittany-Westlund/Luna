using UnityEngine;

public class LunaAnimatorSync : MonoBehaviour
{
    public Animator normalAnimator;
    public Animator glowAnimator;

    void LateUpdate()
    {
        if (!normalAnimator || !glowAnimator) return;

        glowAnimator.speed = normalAnimator.speed;

        foreach (AnimatorControllerParameter p in normalAnimator.parameters)
        {
            switch (p.type)
            {
                case AnimatorControllerParameterType.Float:
                    glowAnimator.SetFloat(p.name, normalAnimator.GetFloat(p.name));
                    break;
                case AnimatorControllerParameterType.Bool:
                    glowAnimator.SetBool(p.name, normalAnimator.GetBool(p.name));
                    break;
                case AnimatorControllerParameterType.Int:
                    glowAnimator.SetInteger(p.name, normalAnimator.GetInteger(p.name));
                    break;
                case AnimatorControllerParameterType.Trigger:
                    if (normalAnimator.GetBool(p.name))
                        glowAnimator.SetTrigger(p.name);
                    break;
            }
        }
    }
}
