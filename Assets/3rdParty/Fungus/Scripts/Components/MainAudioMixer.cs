using UnityEngine;
using UnityEngine.Audio;

namespace Fungus
{
    public class MainAudioMixer : MonoBehaviour
    {
        // 외부에서 접근할 수 있는 순정 Unity AudioMixer
        public AudioMixer Mixer { get; protected set; }
        public AudioMixerGroup BGMGroup { get; protected set; }
        public AudioMixerGroup SFXGroup { get; protected set; }
        public AudioMixerGroup VoiceGroup { get; protected set; }
        public AudioMixerGroup MasterGroup { get; protected set; }

        public virtual void Init()
        {
            // 1. Resources 폴더에서 믹서 에셋 로드
            Mixer = Resources.Load(FungusConstants.FungusAudioMixer) as AudioMixer;

            if (Mixer == null)
            {
                Debug.LogError($"'{FungusConstants.FungusAudioMixer}' 파일을 Resources 폴더에서 찾을 수 없습니다. 파일 이름을 확인해주세요.");
                return;
            }

            // 2. 각 그룹을 안전하게 할당 (배열 크기 체크 추가)
            BGMGroup = GetGroupSafe("BGM");
            SFXGroup = GetGroupSafe("SFX");
            VoiceGroup = GetGroupSafe("Voice");
            MasterGroup = GetGroupSafe("Master");
        }

        protected AudioMixerGroup GetGroupSafe(string groupName)
        {
            if (Mixer == null) return null;

            var groups = Mixer.FindMatchingGroups(groupName);
            if (groups.Length > 0)
            {
                return groups[0];
            }

            Debug.LogWarning($"AudioMixer에서 '{groupName}' 그룹을 찾을 수 없습니다. 이름을 확인해주세요.");
            return null;
        }
    }
}