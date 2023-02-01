namespace tasks;

class TextboxInput
{
    public string Text => text;
    public int BeamIndex => beamIndex;
    public event Action<char> OnAddingCharacter;
    public event Action OnDeletingCharacter;

    const string avaliableCharaters = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM1234567890-=!@#$%^&*()_+[]{};':|\\\",./<>?`~ ";
    const int left = -1;
    const int right = 1;
    int maxTextLength;
    string text;
    int _beamIndex;

    const float keyRepeatWaitTime = 0.5f;
    const float keyRefreshWaitTime = 0.02f;
    float keyRepeatTimer;
    float keyRefreshTimer;
    int arrowKeyDir;

    int lastCharIndex => text.Length-1;
    int beamMaxIndex => lastCharIndex+1;
    int beamIndex {
        get => _beamIndex;
        set {
            _beamIndex = Utils.clamp(value, 0, beamMaxIndex);
        }
    }

    public TextboxInput(GameWindow window, string startText, int maxTextLength)
    {
        this.text = startText;
        this.maxTextLength = maxTextLength;
        window.TextInput += TextInput;
        OnAddingCharacter += (char ch) => {};
        OnDeletingCharacter += () => {};
    }

    public void Update(float dt)
    {
        Controls(dt);
    }

    void Controls(float dt)
    {
        const int left = -1;
        const int right = 1;

        var justPressed = (int dir) => 
        {
            if(Input.IsKeyDown(Keys.LeftControl))
                MoveBeamToWordSide(dir);
            else
                beamIndex += dir;

            arrowKeyDir = dir;
            keyRepeatTimer = keyRepeatWaitTime;
        };

        var keepPressing = (int dir) => 
        {
            keyRepeatTimer -= dt;

            if(keyRepeatTimer <= 0)
            {
                keyRefreshTimer -= dt;

                if(keyRefreshTimer <= 0)
                {
                    if(Input.IsKeyDown(Keys.LeftControl))
                        MoveBeamToWordSide(dir);
                    else
                        beamIndex += dir;

                    keyRefreshTimer = keyRefreshWaitTime;
                }
            }
        };
        
        if(Input.KeyPressed(Keys.Left)) justPressed(left);
        if(Input.KeyPressed(Keys.Right)) justPressed(right);

        if(Input.IsKeyDown(Keys.Left) && arrowKeyDir == left)
            keepPressing(left);

        if(Input.IsKeyDown(Keys.Right) && arrowKeyDir == right) 
            keepPressing(right);
    }

    void TextInput(object? sender, TextInputEventArgs args)
    {
        char character = args.Character;
        const char backspace = '\b';
        const char delete = (char)127;

        if (text.Length > 0)
        {
            if (character == backspace)
            {
                if(Input.IsKeyDown(Keys.LeftControl))
                    EraseWord();
                else 
                {
                    int removeIndex = BeamIndex - 1;
                    if(removeIndex < 0) return;

                    text = text.Remove(removeIndex, 1);
                    beamIndex -= 1;
                }

                OnDeletingCharacter.Invoke();
                return;
            }

            if (character == delete)
            {
                if(beamIndex <= lastCharIndex)
                    text = text.Remove(beamIndex, 1);

                OnDeletingCharacter.Invoke();
                return;
            }
        }

        bool canAddCharacter = (
            avaliableCharaters.Contains(character) &&
            text.Length < maxTextLength
        );

        if(canAddCharacter)
        {
            if(beamIndex > lastCharIndex)
                text = string.Concat(text, character);
            else
                text = text.Insert(beamIndex, character.ToString());

            beamIndex += 1;

            OnAddingCharacter.Invoke(character);
        }
    }

    //Control functionality
    void EraseWord()
    {
        const char space = ' ';
        const int left = -1;

        int endBeamIndex = beamIndex;

        int checkIndex = beamIndex - 1;
        checkIndex = IndexClamp(checkIndex);

        if(text[checkIndex] == space)
            MoveBeamWhile(left, space);
        else
            MoveBeamUntil(left, space);

        text = text.Remove(beamIndex, endBeamIndex - beamIndex);
    }

    void MoveBeamToWordSide(int dir)
    {
        if(text.Length == 0) return;
        const char space = ' ';

        //Check if character at beam (or previous from beam in case of going left) is space
        //If it is them move beam to word 
        int checkIndex = beamIndex;

        if(dir == left) 
            checkIndex -= 1;
        
        checkIndex = IndexClamp(checkIndex);

        if(text[checkIndex] == space)
        {
            MoveBeamWhile(dir, space);

            if(beamIndex == 0)
                return;

            if(beamIndex == beamMaxIndex)
                return;
        }

        //When beam is at word then move it to word side
        MoveBeamUntil(dir, space);
    }

    void MoveBeam(int dir, char ch, bool whileCharacter)
    {
        if(text.Length == 0) return;

        const int left = -1;
        const int right = 1;

        int end = dir == left ? -1 : beamMaxIndex;

        for(int i = beamIndex; i != end; i += dir)
        {
            int checkIndex = i;

            if(dir == left)
            {
                checkIndex -= 1;
                checkIndex = IndexClamp(checkIndex);
            }

            bool breakCondition;

            if(whileCharacter)
                breakCondition = text[checkIndex] != ch;
            else
                breakCondition = text[checkIndex] == ch;

            beamIndex = i;
            
            if(breakCondition)
                return;
        }

        if(dir == left) beamIndex = 0;
        if(dir == right) beamIndex = beamMaxIndex;
    }

    void MoveBeamUntil(int dir, char ch) => MoveBeam(dir, ch, false);
    void MoveBeamWhile(int dir, char ch) => MoveBeam(dir, ch, true);

    int IndexClamp(int index) => Utils.clamp(index, 0, lastCharIndex);
}