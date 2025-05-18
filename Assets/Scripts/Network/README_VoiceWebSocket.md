# 음성 전송 WebSocket 시스템

이 폴더에는 마이크 입력을 캡처하여 WebSocket을 통해 서버로 전송하는 C# 구현체가 있습니다.

## 파이썬 코드 변환

이 코드는 다음과 같은 파이썬 코드를 C#으로 변환한 것입니다:

```python
import asyncio
import websockets
import json
import pyaudio

async def send_audio_stream():
    uri = "ws://192.168.1.129:10050"

    RATE = 16000       # 샘플링 레이트
    CHUNK = 320        # 20ms 단위 (320 samples = 0.02s)
    CHANNELS = 1
    FORMAT = pyaudio.paInt16
    CHUNKS_PER_SEND = int(2 / 0.02)  # 2초 / 0.02초 = 100 청크

    # 마이크 설정
    p = pyaudio.PyAudio()
    stream = p.open(format=FORMAT,
                    channels=CHANNELS,
                    rate=RATE,
                    input=True,
                    frames_per_buffer=CHUNK)

    async with websockets.connect(uri) as websocket:
        print("🔗 서버에 연결됨")

        # 1. voice_tag 전송
        voice_tag = {"type": "voice_tag", "value": "AED_Call119AndRequestAED"}
        await websocket.send(json.dumps(voice_tag))
        print("📤 voice_tag 전송 완료")

        print("🎙️ 실시간 마이크 입력 시작 (2초마다 전송, Ctrl+C로 종료)")

        try:
            buffer = bytearray()
            while True:
                data = stream.read(CHUNK, exception_on_overflow=False)
                buffer.extend(data)

                if len(buffer) >= RATE * 0.1 * 2:  # 2초 x 16000 samples x 2 bytes
                    await websocket.send(buffer)
                    print(f"📤 2초 분량 전송 완료 ({len(buffer)} bytes)")
                    buffer = bytearray()  # 초기화

                await asyncio.sleep(0.001)  # 20ms 간격

        except KeyboardInterrupt:
            print("🛑 마이크 입력 종료됨")
        finally:
            stream.stop_stream()
            stream.close()
            p.terminate()

asyncio.run(send_audio_stream())
```

## C# 구현 클래스

- `VoiceSender.cs`: 마이크 입력을 캡처하여 WebSocket으로 전송하는 클래스
- `VoiceWebSocketClient.cs`: WebSocket 연결 및 메시지 전송을 처리하는 클래스

## 주요 기능

1. WebSocket 서버 연결 (기본 ws://192.168.1.129:10050)
2. 마이크 입력 캡처 (16kHz, 모노)
3. 보이스 태그 전송
4. 오디오 데이터 버퍼링 및 전송
5. 자동 재연결 기능

## 사용 방법

1. `VoiceWebSocketClient` 컴포넌트를 게임 오브젝트에 추가
2. `VoiceSender` 컴포넌트를 게임 오브젝트에 추가
3. Inspector에서 필요한 설정 조정 (서버 URL, 버퍼 시간, 태그 등)
4. 게임 실행 시 자동으로 연결 및 전송 시작
5. ESC 키를 눌러 전송 중지 (Python 코드의 Ctrl+C와 동일한 역할)
