using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class DropItemAnim : MonoBehaviour
{
    private Transform _item;
    private Tweener _tween;

    private GameObject _player;

    private void Start()
    {
        DOTween.Init(false, true, LogBehaviour.Default);

        if (_item == null)
            _item = this.transform;
        if (_player == null)
            _player = GameObject.FindWithTag("Player");

        if (_item == null) return;

        Vector3 jumpTarget = _item.position + new Vector3(Random.Range(-1.5f, 1.5f), 0,
                               Random.Range(-1.5f, 1.5f));

        _item.DOJump(jumpTarget, 1f, 1, 0.5f)
            .SetTarget(gameObject)
            .OnComplete(() =>
            {
                if (this == null || _item == null) return;
                DoMagneticAnim().Forget();
            });
    }

    private async UniTaskVoid DoMagneticAnim()
    {
        if (this == null || _item == null) return;
        if (_player != null)
        {
            Vector3 startPos = _item.position;
            float progress = 0f;

            bool isCanceled = await UniTask.Delay(1000, cancellationToken: this.GetCancellationTokenOnDestroy()).SuppressCancellationThrow();
            if (isCanceled || this == null || _item == null) return;

            // Ease.InSine을 사용하여 progress 변수를 0에서 3로 1초 동안 보간합니다.
            _tween = DOTween.To(() => progress, x => progress = x, 3f, 1f)
                .SetEase(Ease.InSine)
                .SetTarget(gameObject)
                .OnUpdate(() =>
                {
                    if (this == null || _item == null || _player == null)
                    {
                        if (_tween != null)
                        {
                            _tween.Kill();
                            _tween = null;
                        }
                        return;
                    }
                    // 매 프레임 이동하는 플레이어의 현재 위치와 시작 위치 사이를 progress 비율로 보간합니다.
                    _item.position = Vector3.Lerp(startPos, _player.transform.position, progress);
                })
                .OnComplete(() =>
                {
                    if (_tween != null)
                    {
                        _tween.Kill();
                        _tween = null;
                    }
                });
        }
    }

    private void OnDestroy()
    {
        _tween?.Kill();
        if (_item != null)
        {
            _item.DOKill();
        }
    }
}
