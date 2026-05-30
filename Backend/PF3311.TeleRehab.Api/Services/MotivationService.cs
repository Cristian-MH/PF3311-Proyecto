using PF3311.Telerehab.API.Models;

namespace PF3311.Telerehab.API.Services;

public class MotivationService
{
    public string GenerateMessage(MotivationRequest request)
    {
        if (request.CompletedLastTherapy)
        {
            return $"Muy bien, {request.PatientName}. Has avanzado con tu terapia de {request.TherapyName}. Sigue así, cada sesión cuenta para tu recuperación.";
        }

        if (request.Mood.ToLower().Contains("cansado"))
        {
            return $"{request.PatientName}, entiendo que hoy te sientas cansado. Podemos avanzar poco a poco con tu terapia de {request.TherapyName}. Lo importante es mantener la constancia.";
        }

        return $"{request.PatientName}, recuerda realizar tu terapia de {request.TherapyName}. Un pequeño avance hoy puede ayudarte mucho en tu recuperación.";
    }
}