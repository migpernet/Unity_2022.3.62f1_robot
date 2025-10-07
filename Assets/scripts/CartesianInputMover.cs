using UnityEngine;

public class CartesianInputMover : MonoBehaviour
{
    // Velocidade de translação (movimento) em metros por segundo
    [SerializeField] private float speed = 0.5f;

    // Velocidade de rotação em graus por segundo
    [SerializeField] private float rotationSpeed = 45f;

    void Update()
    {
        // Certifique-se de que o script só roda quando a aplicação estiver em foco
        if (!Application.isFocused) return;

        // --- 1. Movimento de Translação (Posição) ---
        
        // WASD ou Setas para mover nos eixos X (Lateral) e Z (Frente/Fundo)
        float moveX = Input.GetAxis("Horizontal"); // A/D ou Setas Esquerda/Direita
        float moveZ = Input.GetAxis("Vertical");   // W/S ou Setas Cima/Baixo

        // QE para mover no eixo Y (Cima/Baixo)
        float moveY = 0f;
        if (Input.GetKey(KeyCode.Q))
        {
            moveY = 1f; // Q: Para cima
        }
        else if (Input.GetKey(KeyCode.E))
        {
            moveY = -1f; // E: Para baixo
        }

        // Vetor de movimento total
        Vector3 movement = new Vector3(moveX, moveY, moveZ) * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);


        // --- 2. Movimento de Rotação (Orientação) ---
        
        // PITCH (X-axis rotation) - Teclas I/K
        if (Input.GetKey(KeyCode.I))
        {
            transform.Rotate(Vector3.right, rotationSpeed * Time.deltaTime, Space.Self);
        }
        else if (Input.GetKey(KeyCode.K))
        {
            transform.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime, Space.Self);
        }

        // YAW (Y-axis rotation) - Teclas J/L
        if (Input.GetKey(KeyCode.J))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }
        else if (Input.GetKey(KeyCode.L))
        {
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.Self);
        }

        // Z, C para Roll (Z-axis rotation) - NOVO
        if (Input.GetKey(KeyCode.U))
        {
            // Rotação em torno do eixo Z positivo (Roll)
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.World);
        }
        else if (Input.GetKey(KeyCode.O))
        {
            // Rotação em torno do eixo Z negativo (Roll reverso)
            transform.Rotate(Vector3.forward, -rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}