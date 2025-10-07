using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Linq;
using UnityEngine.UIElements;

public class JointStateSubscriber : MonoBehaviour
{
    // O tópico que o Gazebo publica o estado do robô
    public string jointStateTopic = "/ur5/joint_states";
    
    // As juntas do seu robô na ordem correta para receber comandos
    public ArticulationBody[] joints;

    void Start()
    {
        // Garante que o array de juntas está preenchido
        if (joints.Length != 6)
        {
            Debug.LogError("O array de juntas não tem 6 elementos. Verifique a atribuição no Inspector.");
            enabled = false;
            return;
        }

        // Assina o tópico de JointState do ROS/Gazebo
        ROSConnection.GetOrCreateInstance().Subscribe<JointStateMsg>(jointStateTopic, ReceiveJointState);
    }

    void ReceiveJointState(JointStateMsg msg)
    {
        // Certifique-se de que o número de posições recebidas corresponde ao número de juntas
        if (msg.position == null || msg.position.Length < joints.Length)
        {
            // O Gazebo pode publicar muitas juntas, mas as 6 primeiras devem ser as do UR5
            // Se o array for muito pequeno, ignoramos.
            return;
        }

        // 1. Converte a lista de double (ROS) para float
        float[] positions = msg.position.Select(p => (float)p).ToArray();
        
        // Reordena as posições se necessário (dependendo da configuração do seu robô no Gazebo) Positions = [positions[2], positions[0], positions[3], positions[4], positions[5]]
        positions = new float[] { positions[3], positions[2], positions[0], positions[4], positions[5], positions[6]};

        Debug.Log("Recebido JointState e reordenado: " + string.Join(", ", positions));



        // 2. Aplica as posições a cada junta local no Unity
        for (int i = 0; i < joints.Length; i++)
        {
            // O Gazebo usa Radianos; o Unity ArticulationBody.target espera Graus.
            // No entanto, para ArticulationBody.target (que é o que estamos definindo), 
            // a unidade esperada é a unidade interna (radianos, a menos que configurado de outra forma).
            // A forma mais segura é aplicar o valor em Radianos e deixar o Unity converter.

            var drive = joints[i].xDrive;

            // ROS envia Radianos. O ArticulationBody Drive espera Radianos.
            drive.target = positions[i] * Mathf.Rad2Deg; // Aplicando a conversão para Radianos se necessário,
                                                         // mas o ArticulationBody lida com Radianos internamente.
                                                         // Vamos usar Rad2Deg para forçar a conversão que o IK precisava,
                                                         // se necessário, mas para feedback direto, radianos é mais comum.

            // TESTE A SEGUIR: o IK usa Rad2Deg no lado C#, vamos manter isso para consistência:
            drive.target = positions[i] * Mathf.Rad2Deg;

            joints[i].xDrive = drive;
        }
    }
}
