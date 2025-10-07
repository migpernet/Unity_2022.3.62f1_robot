using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std; 
using RosMessageTypes.BuiltinInterfaces;
using System;

public class CustomIKController : MonoBehaviour
{
    [SerializeField] private Transform targetObject;
    [SerializeField] private string poseTopicName = "unity/target_pose";

    // Posição incial definida no Inspector do Unity
    [SerializeField] private Vector3 initialPosition;
    // Rotação em Euler inicial definida no Inspector do Unity
    [SerializeField] private Vector3 initialRotation; 

    private ROSConnection ros;
    private PoseStampedMsg poseMessage;

    void Start()
    {
        // Garante que apenas este script publica a pose
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(poseTopicName);
        poseMessage = new PoseStampedMsg();

        // Posicionar o targetObject na posição inicial definida pelo usuário no Inspector do Unity
        if (targetObject != null)
        {
            targetObject.position = transform.TransformPoint(-initialPosition.y, initialPosition.z, initialPosition.x);
            Quaternion initialRotationQuat = Quaternion.Euler(initialRotation);

            // targetObject.rotation = transform.rotation * initialRotation;
            Debug.Log($"Target Object posicionado na posição inicial: {initialPosition}, rotação inicial: {initialRotation}");  
        }
        else
        {
            Debug.LogError("Target Object não está atribuído no Inspector.");
        }
 

    }

    void FixedUpdate()
    {
        if (targetObject == null) return;

        // --- 1. Calcular a Pose Relativa ---
        // 'transform' é a pose da base do robô (o objeto que este script está anexado)
        Transform robotBase = transform;

        // Converte a pose do mundo para a pose relativa à base
        Vector3 localPosition = robotBase.InverseTransformPoint(targetObject.position);
        Quaternion localRotation = Quaternion.Inverse(robotBase.rotation) * targetObject.rotation;


        // // --- 2. TRANSFORMAÇÃO DE COORDENADAS (UNITY Y-up PARA ROS Z-up) ---

        // // POSIÇÃO: X permanece; Y (Unity) vira Z (ROS); Z (Unity) vira -Y (ROS)
        // // A inversão de sinal no Z/Y é crucial para manter a regra da mão direita.
        // poseMessage.pose.position.x = localPosition.x;  // poseMessage.pose.position.y (ROS) -> localPosition.x (Unity)
        // poseMessage.pose.position.y = localPosition.z; // poseMessage.pose.position.y (ROS) -> localPosition.x (Unity)
        // poseMessage.pose.position.z = -localPosition.y;  // poseMessage.pose.position.y (ROS) -> localPosition.x (Unity)

        // // ORIENTAÇÃO: O Quaternion também precisa ser rotacionado.
        // // O método mais seguro para quatérnions é criar a rotação que alinha os eixos
        // // e aplicar essa rotação ao Quaternion do alvo.
        // Quaternion rosAlignment = new Quaternion(-localRotation.x, -localRotation.z, -localRotation.y, localRotation.w);        


        // --- 3. Publicar a Mensagem ---

        // Calcula e atribui o timestamp (essencial para ROS)
        float currentTime = Time.realtimeSinceStartup;
        uint secs = (uint)Mathf.Floor(currentTime);
        uint nsecs = (uint)((currentTime - secs) * 1000000000);

        poseMessage.header.stamp = new TimeMsg
        {
            sec = secs,
            nanosec = nsecs
        };

        poseMessage.header.frame_id = "base_link"; // O frame de referência que o ROS espera

        // Preenche os campos de posição e orientação (Quaternion)
        poseMessage.pose.position.x = localPosition.z;
        poseMessage.pose.position.y = -localPosition.x;
        poseMessage.pose.position.z = localPosition.y;

        poseMessage.pose.orientation.x = localRotation.x;
        poseMessage.pose.orientation.y = localRotation.y;
        poseMessage.pose.orientation.z = localRotation.z;
        poseMessage.pose.orientation.w = localRotation.w;

        // Publica a mensagem de pose
        ros.Publish(poseTopicName, poseMessage);

        // posicionar o targetObject no Unity
        targetObject.position = transform.TransformPoint(new Vector3((float)-poseMessage.pose.position.y, (float)poseMessage.pose.position.z, (float)poseMessage.pose.position.x));
        targetObject.rotation = transform.rotation * new Quaternion((float)poseMessage.pose.orientation.x, (float)poseMessage.pose.orientation.y, (float)poseMessage.pose.orientation.z, (float)poseMessage.pose.orientation.w);
 
        Debug.Log($"Publicando Pose Relativa ao Robô no Unity: Pos({poseMessage.pose.position.x:F2}, {poseMessage.pose.position.y:F2}, {poseMessage.pose.position.z:F2})");        


        // // Mostrar a pose no Inpector para depuração
        // Debug.Log($"Publicando Pose Relativa ao Robô no Unity: Pos({poseMessage.pose.position.x:F2}, {poseMessage.pose.position.y:F2}, {poseMessage.pose.position.z:F2}) " +
        //           $"Orient({poseMessage.pose.orientation.x:F2}, {poseMessage.pose.orientation.y:F2}, {poseMessage.pose.orientation.z:F2}, {poseMessage.pose.orientation.w:F2})");
    }
}