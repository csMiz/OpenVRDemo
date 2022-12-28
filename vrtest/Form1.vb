
Imports SharpDX
Imports SharpDX.Direct3D
Imports SharpDX.Direct3D11
Imports SharpDX.DXGI
Imports SharpDX.Mathematics.Interop

Imports Valve.VR

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click

        Dim errs As EVRInitError = EVRInitError.None
        Dim ivrsys As Valve.VR.CVRSystem = Valve.VR.OpenVR.Init(errs, EVRApplicationType.VRApplication_Scene)

        Dim vrc As CVRCompositor = Valve.VR.OpenVR.Compositor
        If vrc Is Nothing Then
            Debug.WriteLine("vrc Is Nothing")
        End If

        Debug.WriteLine(errs.ToString)
        Debug.WriteLine("hello")

        Dim adapter_id As Integer = 0
        ivrsys.GetDXGIOutputInfo(adapter_id)
        Debug.WriteLine(adapter_id)

        Dim fac As Factory2 = New Factory2()
        Dim adapter As Adapter1 = fac.GetAdapter1(adapter_id)
        Dim d3ddevice As Direct3D11.Device = New Direct3D11.Device(adapter, DeviceCreationFlags.BgraSupport, FeatureLevel.Level_11_0)
        Dim d3ddevice1 As Direct3D11.Device1 = d3ddevice.QueryInterface(Of SharpDX.Direct3D11.Device1)()
        Dim d3dcontext = d3ddevice1.ImmediateContext.QueryInterface(Of SharpDX.Direct3D11.DeviceContext1)()
        Dim dxgiDevice2 As SharpDX.DXGI.Device2 = d3ddevice1.QueryInterface(Of SharpDX.DXGI.Device2)()

        Dim description As DXGI.SwapChainDescription1 = New DXGI.SwapChainDescription1()
        With description
            .Width = 1280
            .Height = 800
            .Format = DXGI.Format.B8G8R8A8_UNorm  '32 bit RGBA color.
            .Stereo = False
            .SampleDescription = New DXGI.SampleDescription(1, 0)    'No multisampling.
            .Usage = DXGI.Usage.RenderTargetOutput     'Use the swap chain as a render target.
            .BufferCount = 2            'Enable double buffering to prevent flickering.
            .Scaling = DXGI.Scaling.Stretch        'No scaling.->stretch
            .SwapEffect = DXGI.SwapEffect.FlipSequential           'Flip between both buffers.
            .Flags = DXGI.SwapChainFlags.AllowModeSwitch
        End With
        Dim swapchain = New DXGI.SwapChain1(fac, d3ddevice, Me.Handle, description, Nothing)

        Dim d3dRenderRTV As Direct3D11.RenderTargetView
        Dim rtvtex2d As Texture2D = Direct3D11.Resource.FromSwapChain(Of Direct3D11.Texture2D)(swapchain, 0)
        d3dRenderRTV = New Direct3D11.RenderTargetView(d3ddevice, rtvtex2d)

        Dim d2dDevice As SharpDX.Direct2D1.Device = New SharpDX.Direct2D1.Device(dxgiDevice2)
        Dim d2dDeviceContext = New SharpDX.Direct2D1.DeviceContext(d2dDevice, SharpDX.Direct2D1.DeviceContextOptions.EnableMultithreadedOptimizations)

        Dim bitmapProperties As Direct2D1.BitmapProperties1 = New Direct2D1.BitmapProperties1(
            New Direct2D1.PixelFormat(SharpDX.DXGI.Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
            96, 96, Direct2D1.BitmapOptions.Target Or Direct2D1.BitmapOptions.CannotDraw)
        Dim backBuffer As DXGI.Surface = swapchain.GetBackBuffer(Of DXGI.Surface)(0)
        Dim d2dBitmap = New Direct2D1.Bitmap1(d2dDeviceContext, backBuffer, bitmapProperties)

        Dim dwFac As DirectWrite.Factory = New DirectWrite.Factory
        Dim font As New DirectWrite.TextFormat(dwFac, "Microsoft Yahei", 24)
        Dim brush1 As New Direct2D1.SolidColorBrush(d2dDeviceContext, New RawColor4(1, 1, 1, 1))

        ' ----------- draw --------------
        For i = 1 To 60 * 60
            d3dcontext.ClearRenderTargetView(d3dRenderRTV, New RawColor4(0.2, 0.2, 0.6, 1.0))
            d2dDeviceContext.Target = d2dBitmap
            d2dDeviceContext.BeginDraw()
            d2dDeviceContext.DrawText("Hello VR on Steam!", font, New RawRectangleF(400, 400, 800, 600), brush1)

            d2dDeviceContext.EndDraw()

            If vrc IsNot Nothing Then
                Dim tr1() As TrackedDevicePose_t = {New TrackedDevicePose_t()}
                Dim tr2() As TrackedDevicePose_t = {New TrackedDevicePose_t()}
                errs = vrc.WaitGetPoses(tr1, tr2)
                If errs <> EVRInitError.None Then Debug.WriteLine(errs)

                Dim bound As VRTextureBounds_t = New VRTextureBounds_t
                With bound
                    .uMax = 1.0
                    .uMin = 0.0
                    .vMax = 1.0
                    .vMin = 0.0
                End With

                Dim tex1 As Texture_t
                tex1.handle = rtvtex2d.NativePointer
                tex1.eType = ETextureType.DirectX
                tex1.eColorSpace = EColorSpace.Gamma

                errs = vrc.Submit(EVREye.Eye_Left, tex1, bound, EVRSubmitFlags.Submit_Default)
                If errs <> EVRInitError.None Then Debug.WriteLine(errs)
                errs = vrc.Submit(EVREye.Eye_Right, tex1, bound, EVRSubmitFlags.Submit_Default)
                If errs <> EVRInitError.None Then Debug.WriteLine(errs)

                swapchain.Present(1, PresentFlags.None)
            End If
        Next



    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        OpenVR.Shutdown()
    End Sub
End Class
