using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using Unity.Robotics.UrdfImporter;
using System.Linq;

public class JointControlGUI : MonoBehaviour
{
    // Configure o tópico para o seu nó ROS se inscrever
    public string topicName = "unity/joint_command";

    public UrdfJoint[] urdfJoints;
    public ArticulationBody[] robotJoints;
    public ArticulationBody gripperJoint;

    private ROSConnection ros;
    private JointStateMsg jointState;

    private float[] jointValues = new float[7]; // 6 juntas do UR5 + 1 da garra

    private void Start()
    {
        // Obtém a instância da conexão ROS usando a propriedade 'instance'
        ros = ROSConnection.GetOrCreateInstance();
        // Registra o publisher
        ros.RegisterPublisher<JointStateMsg>(topicName);

        // Inicializa a mensagem
        jointState = new JointStateMsg();

        // Encontra todos os ArticulationBody do robô e da garra
        var allUrdfJoints = GetComponentsInChildren<UrdfJoint>();
        robotJoints = new ArticulationBody[6];
        int jointIndex = 0;
        foreach (var urdfJoint in allUrdfJoints)
        {
            if (urdfJoint.JointType != UrdfJoint.JointTypes.Fixed)
            {
                if (urdfJoint.jointName.Contains("robotiq_85_left_knuckle"))
                {
                    // Trata a garra separadamente
                    gripperJoint = urdfJoint.GetComponent<ArticulationBody>();
                }
                else if (jointIndex < 6)
                {
                    robotJoints[jointIndex] = urdfJoint.GetComponent<ArticulationBody>();
                    jointIndex++;
                }
            }
        }

        

        // Adicione esta linha:
        if (gripperJoint != null)
        {
            Debug.Log("Junta da garra encontrada: " + gripperJoint.gameObject.name);
        }
        else
        {
            Debug.LogWarning("Junta da garra NÃO encontrada!");
        }
    


    }

    private void FixedUpdate()
    {
        // Envia o comando para cada ArticulationBody na simulação do Unity
        // UR5
        for (int i = 0; i < 6; i++)
        {
            var jointDrive = robotJoints[i].xDrive;
            jointDrive.target = jointValues[i];
            robotJoints[i].xDrive = jointDrive;
        }

        // Garra
        if (gripperJoint != null)
        {

            // Adicione esta linha para verificar o valor do slider
            Debug.Log("Valor do slider da garra: " + jointValues[6]);


            var gripperDrive = gripperJoint.xDrive;
            gripperDrive.target = jointValues[6];
            gripperJoint.xDrive = gripperDrive;


            // Adicione esta linha para verificar o valor do target
            Debug.Log("Valor do xDrive.target: " + gripperJoint.xDrive.target);
        }

        // Prepara e publica a mensagem ROS
        List<float> positions = new List<float>();
        for (int i = 0; i < 6; i++)
        {
            positions.Add(jointValues[i] * Mathf.Deg2Rad); // Converte para radianos para ROS
        }
        positions.Add(jointValues[6]); // Valor da garra

        jointState.position = positions.Select(x => (double)x).ToArray();

        ros.Publish(topicName, jointState);
    }

    void OnGUI()
    {
        int boundary = 30;
        int labelHeight = 20;
        GUI.skin.label.fontSize = GUI.skin.box.fontSize = GUI.skin.button.fontSize = 15;
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

        // Sliders para as 6 juntas do UR5
        for (int i = 0; i < 6; i++)
        {
            GUI.Label(new Rect(boundary, boundary + i * labelHeight * 2, labelHeight * 4, labelHeight), "Joint " + (i + 1) + ": ");
            jointValues[i] = GUI.HorizontalSlider(new Rect(boundary + labelHeight * 4, boundary + i * labelHeight * 2 + labelHeight / 4, labelHeight * 5, labelHeight), jointValues[i], -180f, 180f);
        }

        // Slider para a Garra
        GUI.Label(new Rect(boundary, boundary + 6 * labelHeight * 2, labelHeight * 4, labelHeight), "Gripper:");
        jointValues[6] = GUI.HorizontalSlider(new Rect(boundary + labelHeight * 4, boundary + 6 * labelHeight * 2 + labelHeight / 4, labelHeight * 5, labelHeight), jointValues[6], 0f, 0.8f);

    }


}