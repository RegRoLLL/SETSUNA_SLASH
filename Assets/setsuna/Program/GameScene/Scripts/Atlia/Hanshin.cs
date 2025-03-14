using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hanshin : SetsunaSlashScript
{
    [SerializeField] private List<GameObject> predefinedObjects; // ���O�ɐݒ肷��\���p�I�u�W�F�N�g���X�g
    [SerializeField] private float bladeTimer = 0f; // Blade ���G��Ă��鎞��
    [SerializeField] private float mincount;
    private int objectIndex = 0; // ���ɕ\������I�u�W�F�N�g�̃C���f�b�N�X

    private void Start()
    {
        // ���O�ɐݒ肳�ꂽ�I�u�W�F�N�g��S�Ĕ�\���ɂ���
        foreach (var obj in predefinedObjects)
        {
            obj.SetActive(false);
        }
    }

    private void Update()
    {       
            bladeTimer += Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == bladeLayer)
        {
            bladeTimer = 0f; // �^�C�}�[�����Z�b�g

        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == bladeLayer)
        {   

            if (bladeTimer >= mincount) 
            {
                ShowNextObject();
            }

        }
    }


    private void ShowNextObject()
    {
        if (predefinedObjects.Count == 0) return; // �I�u�W�F�N�g���X�g����Ȃ牽�����Ȃ�

        // ���̃I�u�W�F�N�g��\��
        predefinedObjects[objectIndex].SetActive(true);

        objectIndex = (objectIndex + 1) % predefinedObjects.Count;
    }
}
