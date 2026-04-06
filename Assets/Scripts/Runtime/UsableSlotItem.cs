using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// # ������ ��� �˰������� ���� ����� �� ����.

[System.Serializable]
public class HoeItem : Item
{
    public int currentDuration = 100; // ������ �⺻��

    public HoeItem(short id, short count) : base(id, count)
    {
        // ���� �ʱ�ȭ
    }

    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
        #if UNITY_EDITOR

            Debug.Log("�������� ���ؼ� ���̸� �ֵ� �� �����ϴ�!");
        #endif
            //TODO: ���Ұ� UI �˾� ����
            return;
        }

        // ��¡ �ð��� ���� ���� ��� ���� (�Ʒ� PlayerController�� ����)
        ExecuteHoeAction(param);

    }

    private void ExecuteHoeAction(UseParam param)
    {
        // 1. ��¡ �ܰ迡 ���� ���� ���� (��: 1�ܰ�=1x1, 2�ܰ�=1x3 ...)
        // 2. SelectionArea�� �̿��� Ÿ�� ����
        // 3. ObjectPool���� �� ������ ���� ������ ��ġ

        currentDuration--; // ��� �� ������ ����

        #if UNITY_EDITOR
        Debug.Log($"������ ����! ���� ������: {currentDuration}");
        #endif
    }
}

public class HammerItem : Item
{
    public int currentDuration = 100;
    public HammerItem(short id, short count) : base(id, count)
    {

    }
    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
            #if UNITY_EDITOR
            Debug.Log("�������� ���ؼ� ��ġ�� �ֵ� �� �����ϴ�!");
            #endif 
            return;
        }

        // ��¡ �ð��� ���� ���� ��� ���� (�Ʒ� PlayerController�� ����)
        ExecuteHammerAction(param);
    }

    private void ExecuteHammerAction(UseParam param)
    {
        // 1. ��¡ �ܰ迡 ���� ���� ���� (��: 1�ܰ�=1x1, 2�ܰ�=1x3 ...)
        // 2. SelectionArea�� �̿��� Ÿ�� ����
        // 3. ObjectPool���� �� ������ ���� ������ ��ġ



        currentDuration--; // ��� �� ������ ����
        #if UNITY_EDITOR
        Debug.Log($"��ġ�� ����! ���� ������: {currentDuration}");
        #endif
    }
}

public class WateringCanItem : Item
{
    public int currentDuration = 100;
    public WateringCanItem(short id, short count) : base(id, count)
    {

    }
    public override void OnUse(UseParam param)
    {
        if (currentDuration <= 0)
        {
            #if UNITY_EDITOR
            Debug.Log("�������� ���ؼ� ���Ѹ����� ��� �� �� �����ϴ�!");
            #endif
            //TODO
            return;
        }

        // ��¡ �ð��� ���� ���� ��� ���� (�Ʒ� PlayerController�� ����)
        ExcuseWateringCanAction(param);
    }

    private void ExcuseWateringCanAction(UseParam param)
    {
        // 1. ��¡ �ܰ迡 ���� ���� ���� (��: 1�ܰ�=1x1, 2�ܰ�=1x3 ...)
        // 2. SelectionArea�� �̿��� Ÿ�� ����
        // 3. ObjectPool���� �� ������ ���� ������ ��ġ
        #if UNITY_EDITOR
        currentDuration--; // ��� �� ������ ����
        Debug.Log($"�ؼ����Ŀ���! ���� ������: {currentDuration}");
        #endif
    }


}
public class ConsumableSlotItem : Item
{
    public ConsumableSlotItem(short id, short count) : base(id, count)
    {
    }
    public override void OnUse(UseParam param)
    {
        // ��¡ �ð��� ���� ���� ��� ���� (�Ʒ� PlayerController�� ����)
        ExcuseWateringCanAction(param);
        if (amount == 0)
        {
            Debug.Log("�������� ��� ��� �߽��ϴ�!");
            Cleanup();
            return;
        }
    }

    private void ExcuseWateringCanAction(UseParam param)
    {
        // 1. ��¡ �ܰ迡 ���� ���� ���� (��: 1�ܰ�=1x1, 2�ܰ�=1x3 ...)
        // 2. SelectionArea�� �̿��� Ÿ�� ����
        // 3. ObjectPool���� �� ������ ���� ������ ��ġ

        amount--; // ��� �� ������ ����

        #if UNITY_EDITOR
        Debug.Log($"������ ���. ���� ������ ����: {amount}");
        #endif
    }
}

