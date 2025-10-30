using UnityEngine;

/// <summary>
/// ahems wjr tkdxodml cntkd rlqks zmffotm
/// rkr tkdxosms Enter/Update/Exit tnaudwnrlfmf xhdgo ehdwkr
/// Contextdml rhddb epdlxj tkdyd
/// </summary>
public abstract class BaseState
{
    /// <summary>
    /// tkdxork tkdydgkf zjsxprtmxm(rhddb epdlxj alc dbxlf aptjem wprhd)
    /// </summary>
    protected StateManager _context;//guswo rocpdml tkdxo rhksflwk(qusrud/wjsdl/rhddb rkqt wjqrms)

    /// <summary>
    /// tkdxo dlstmxjstmrk tkdydgkf zjsxprtmxm wndlq
    /// ahems tkdxosms wjsghks tl ehddlf zjsxprtmxm rhddb
    /// </summary>
    /// <param name="context"></param>
    public void SetContext(StateManager context)
    {
        _context = context;
    }

    /// <summary>
    /// tkdxo wlsdlq tl ghcnf
    /// doslapdltus/tkdnsem/chrl xkdlaj tpxld emd tngod
    /// </summary>
    public virtual void OnEnter()
    {
        //vktod zmffotmdptj vlfdy tl rngus
    }

    /// <summary>
    /// ao vmfpdla ghcnf
    /// tkdxo rhdb fhwlr tngod
    /// vlfdytl _context.RequestStateChange(...)fh wjsdl dycjd
    /// </summary>
    public virtual void OnUpdate(float dt)
    {
        //vktod zmffotmdptj vlfdy tl rngus
    }

    /// <summary>
    /// tkdxo whdfy wlrwjsdp ghcnf
    /// xkdlaj/ vmfform wjdfl
    /// </summary>
    public virtual void OnExit()
    {
        //vktod zmffotmdptj vlfdy tl rngus
    }

    /// <summary>
    /// elqjrmdyd tkdxoaud qksghks gkatn
    /// </summary>
    /// <returns></returns>
    public abstract string Name();
}