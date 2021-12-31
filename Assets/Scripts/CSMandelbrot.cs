using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CSMandelbrot : MonoBehaviour
{
    public static readonly double WIDTH = 5;
    public static readonly double HEIGHT = 2.5;
    public static readonly double R = -3.25;
    public static readonly double I = -1.5;

    //Shader resources
    public ComputeShader shader;
    ComputeBuffer buffer;
    RenderTexture renderTexture;
    public RawImage rawImage;

    //GUI Resources
    public TextMeshProUGUI real, imag, w, h, ite, frame, mouseX, mouseY;
    public int increment = 3;
    public float zoomSpeed = 1.5f;


    //Mandelbrot parameters
    double width = 5;
    double height = 2.5;
    public double rStart = -3.25;
    public double iStart = -1.4;
    int maxIteration = 1024*4;


    //Data for the Compute Shader
    public struct DataStruct
    {
        public double w, h, r, i;
        public int screenWidth, screenHeight;
    }

    DataStruct[] data = new DataStruct[1];


    // Start is called before the first frame update
    void Start()
    {
        height = width * Screen.height / Screen.width;

        data[0] = new DataStruct
        {
            w = width,
            h = height,
            r = rStart,
            i = iStart,
            screenWidth = Screen.width,
            screenHeight = Screen.height
        };

        buffer = new ComputeBuffer(1, 40);

        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        UpdateTexture();
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            StartCoroutine(ZoomingIn());
        }

        if (Input.GetMouseButton(1))
        {
            StartCoroutine(ZoomingOut());
        }

        if (Input.GetMouseButtonDown(2))
        {
            CenterScreen();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetView();
        }

        double xDistFromCenter = (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * width;
        double yDistFromCenter = (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * height;

        //rStart += (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * width;
        //iStart += (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * height;

        mouseX.text = "mouse.x: " + xDistFromCenter; //Input.mousePosition.x.ToString();
        mouseY.text = "mouse.y: " + yDistFromCenter; // Input.mousePosition.y.ToString();

    }

    void ResetView()
    {
        width  = WIDTH;
        height = WIDTH * Screen.height / Screen.width;
        rStart = R;
        iStart = I;

        data[0].w = width;
        data[0].h = height;
        data[0].r = rStart;
        data[0].i = iStart;
        UpdateTexture();

    }

    IEnumerator ZoomingIn()
    {
        yield return StartCoroutine(ZoomIn());
    }

    IEnumerator ZoomingOut()
    {
        yield return StartCoroutine(ZoomOut());
    }


    IEnumerator ZoomIn()
    {
        maxIteration = Mathf.Max(100, maxIteration + increment);

        double xDistFromCenter = (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * width;
        double yDistFromCenter = (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * height;

        double wFactor = width * zoomSpeed * Time.deltaTime;
        double hFactor = height * zoomSpeed * Time.deltaTime;
        width -= wFactor;
        height -= hFactor;
        rStart += wFactor / 2.0 + xDistFromCenter * 0.1;
        iStart += hFactor / 2.0 + yDistFromCenter * 0.1;

        data[0].w = width;
        data[0].h = height;
        data[0].r = rStart;
        data[0].i = iStart;

        UpdateTexture();

        yield return null;
    }

    IEnumerator ZoomOut()
    {
        maxIteration = Mathf.Max(100, maxIteration - increment);

        double xDistFromCenter = (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * width;
        double yDistFromCenter = (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * height;

        double wFactor = width * zoomSpeed * Time.deltaTime;
        double hFactor = height * zoomSpeed * Time.deltaTime;
        width += wFactor;
        height += hFactor;
        rStart -= wFactor / 2.0 - xDistFromCenter * 0.1;
        iStart -= hFactor / 2.0 - yDistFromCenter * 0.1;

        data[0].w = width;
        data[0].h = height;
        data[0].r = rStart;
        data[0].i = iStart;

        UpdateTexture();

        yield return null;
    }


    void CenterScreen()
    {
        rStart += (Input.mousePosition.x - (Screen.width / 2.0)) / Screen.width * width;
        iStart += (Input.mousePosition.y - (Screen.height / 2.0)) / Screen.height * height;

        data[0].r = rStart;
        data[0].i = iStart;

        UpdateTexture();
    }


    void UpdateTexture()
    {
        int kernelHandle = shader.FindKernel("CSMain");

        buffer.SetData(data);
        shader.SetBuffer(kernelHandle, "buffer", buffer);

        shader.SetInt("iterations", maxIteration);
        shader.SetTexture(kernelHandle, "Result", renderTexture);

        shader.Dispatch(kernelHandle, Screen.width / 32, Screen.height / 32, 1);

        RenderTexture.active = renderTexture;
        rawImage.material.mainTexture = renderTexture;

        real.text = "Real Part: " + rStart.ToString();
        imag.text = "Imaginary Part: " + iStart.ToString();
        w.text = "Width: " + width.ToString();
        h.text = "Height: " + height.ToString();
        ite.text = "Iterations: " + maxIteration.ToString();

        frame.text = Time.deltaTime.ToString();
    }


    void OnDestroy()
    {
        buffer.Dispose();
    }

}
